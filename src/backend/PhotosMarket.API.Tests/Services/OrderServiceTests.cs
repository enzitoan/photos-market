using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhotosMarket.API.Services;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.Models;
using PhotosMarket.API.DTOs;
using PhotosMarket.API.Configuration;

namespace PhotosMarket.API.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IDownloadLinkRepository> _downloadLinkRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IPhotographerSettingsRepository> _photographerSettingsRepositoryMock;
    private readonly Mock<ILogger<OrderService>> _loggerMock;
    private readonly OrderService _orderService;
    private readonly ApplicationSettings _appSettings;

    public OrderServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _downloadLinkRepositoryMock = new Mock<IDownloadLinkRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _photographerSettingsRepositoryMock = new Mock<IPhotographerSettingsRepository>();
        _loggerMock = new Mock<ILogger<OrderService>>();

        _appSettings = new ApplicationSettings
        {
            PhotoPricePerUnit = 5000,
            Currency = "CLP",
            BulkDiscountMinPhotos = 10,
            BulkDiscountPercentage = 15
        };

        _orderService = new OrderService(
            _orderRepositoryMock.Object,
            _downloadLinkRepositoryMock.Object,
            _emailServiceMock.Object,
            _photographerSettingsRepositoryMock.Object,
            Options.Create(_appSettings),
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_CreatesOrderSuccessfully()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "test@example.com";
        var userName = "John Doe";
        var photos = new List<OrderPhotoDto>
        {
            new OrderPhotoDto
            {
                PhotoId = "photo1",
                MediaItemId = "media1",
                Filename = "photo1.jpg",
                BaseUrl = "https://example.com/photo1.jpg",
                AlbumId = "album1",
                AlbumTitle = "Test Album"
            },
            new OrderPhotoDto
            {
                PhotoId = "photo2",
                MediaItemId = "media2",
                Filename = "photo2.jpg",
                BaseUrl = "https://example.com/photo2.jpg",
                AlbumId = "album1",
                AlbumTitle = "Test Album"
            }
        };

        var createdOrder = new Order
        {
            Id = "order123",
            UserId = userId,
            UserEmail = userEmail,
            UserName = userName,
            Photos = photos.Select(p => new OrderPhoto
            {
                PhotoId = p.PhotoId,
                MediaItemId = p.MediaItemId,
                Filename = p.Filename,
                BaseUrl = p.BaseUrl,
                Price = 5000,
                AlbumId = p.AlbumId,
                AlbumTitle = p.AlbumTitle
            }).ToList(),
            Subtotal = 10000,
            TotalAmount = 10000,
            Currency = "CLP",
            Status = OrderStatus.AwaitingPayment,
            CreatedAt = DateTime.UtcNow
        };

        _photographerSettingsRepositoryMock
            .Setup(r => r.GetSettingsAsync())
            .ReturnsAsync((PhotographerSettings?)null);

        _orderRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        _emailServiceMock
            .Setup(s => s.SendOrderAwaitingPaymentEmailAsync(It.IsAny<Order>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, userEmail, userName, photos);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Photos.Should().HaveCount(2);
        result.TotalAmount.Should().Be(10000);
        result.Status.Should().Be(OrderStatus.AwaitingPayment);

        _orderRepositoryMock.Verify(r => r.CreateAsync(It.Is<Order>(o =>
            o.UserId == userId &&
            o.UserEmail == userEmail &&
            o.Photos.Count == 2
        )), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithBulkDiscount_AppliesDiscountCorrectly()
    {
        // Arrange
        var userId = "user123";
        var userEmail = "test@example.com";
        var userName = "John Doe";
        var photos = Enumerable.Range(1, 10).Select(i => new OrderPhotoDto
        {
            PhotoId = $"photo{i}",
            MediaItemId = $"media{i}",
            Filename = $"photo{i}.jpg",
            BaseUrl = $"https://example.com/photo{i}.jpg",
            AlbumId = "album1",
            AlbumTitle = "Test Album"
        }).ToList();

        // Total: 10 photos * 5000 = 50000
        // Discount: 15% of 50000 = 7500
        // Final: 50000 - 7500 = 42500

        var createdOrder = new Order
        {
            Id = "order123",
            UserId = userId,
            UserEmail = userEmail,
            UserName = userName,
            Photos = photos.Select(p => new OrderPhoto
            {
                PhotoId = p.PhotoId,
                MediaItemId = p.MediaItemId,
                Filename = p.Filename,
                BaseUrl = p.BaseUrl,
                Price = 5000,
                AlbumId = p.AlbumId,
                AlbumTitle = p.AlbumTitle
            }).ToList(),
            Subtotal = 50000,
            DiscountPercentage = 15,
            DiscountAmount = 7500,
            TotalAmount = 42500,
            Currency = "CLP",
            Status = OrderStatus.AwaitingPayment,
            CreatedAt = DateTime.UtcNow
        };

        _photographerSettingsRepositoryMock
            .Setup(r => r.GetSettingsAsync())
            .ReturnsAsync((PhotographerSettings?)null);

        _orderRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .ReturnsAsync(createdOrder);

        _emailServiceMock
            .Setup(s => s.SendOrderAwaitingPaymentEmailAsync(It.IsAny<Order>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.CreateOrderAsync(userId, userEmail, userName, photos);

        // Assert
        result.Should().NotBeNull();
        result.Photos.Should().HaveCount(10);
        result.Subtotal.Should().Be(50000);
        result.DiscountPercentage.Should().Be(15);
        result.DiscountAmount.Should().Be(7500);
        result.TotalAmount.Should().Be(42500);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var userId = "user123";
        var orderId = "order123";
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            UserEmail = "test@example.com",
            UserName = "John Doe",
            Photos = new List<OrderPhoto>(),
            TotalAmount = 5000,
            Status = OrderStatus.AwaitingPayment
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_OrderNotFound_ReturnsNull()
    {
        // Arrange
        var userId = "user123";
        var orderId = "nonexistent";

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId, userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserOrdersAsync_ExistingOrders_ReturnsOrdersList()
    {
        // Arrange
        var userId = "user123";
        var orders = new List<Order>
        {
            new Order
            {
                Id = "order1",
                UserId = userId,
                UserEmail = "test@example.com",
                Photos = new List<OrderPhoto>(),
                TotalAmount = 10000,
                Status = OrderStatus.AwaitingPayment
            },
            new Order
            {
                Id = "order2",
                UserId = userId,
                UserEmail = "test@example.com",
                Photos = new List<OrderPhoto>(),
                TotalAmount = 5000,
                Status = OrderStatus.Completed
            }
        };

        _orderRepositoryMock
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(orders);

        // Act
        var result = await _orderService.GetUserOrdersAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Id == "order1" && o.Status == OrderStatus.AwaitingPayment);
        result.Should().Contain(o => o.Id == "order2" && o.Status == OrderStatus.Completed);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_ValidOrder_UpdatesOrderStatus()
    {
        // Arrange
        var userId = "user123";
        var orderId = "order123";
        var paymentReference = "PAY-12345";
        var order = new Order
        {
            Id = orderId,
            UserId = userId,
            UserEmail = "test@example.com",
            UserName = "John Doe",
            Photos = new List<OrderPhoto>(),
            TotalAmount = 5000,
            Status = OrderStatus.AwaitingPayment
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        _emailServiceMock
            .Setup(s => s.SendOrderConfirmedEmailAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderService.ConfirmPaymentAsync(orderId, userId, paymentReference);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.PaymentConfirmed);
        result.PaymentReference.Should().Be(paymentReference);
        result.PaymentConfirmedAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Id == orderId && 
            o.Status == OrderStatus.PaymentConfirmed &&
            o.PaymentReference == paymentReference
        )), Times.Once);
    }

    [Fact]
    public async Task CompleteOrderAsync_ValidOrder_MarksAsCompleted()
    {
        // Arrange
        var orderId = "order123";
        var order = new Order
        {
            Id = orderId,
            UserId = "user123",
            UserEmail = "test@example.com",
            UserName = "John Doe",
            Photos = new List<OrderPhoto>(),
            TotalAmount = 5000,
            Status = OrderStatus.PaymentConfirmed
        };

        _orderRepositoryMock
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _orderRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) => o);

        // Act
        var result = await _orderService.CompleteOrderAsync(orderId);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(OrderStatus.Completed);
        result.CompletedAt.Should().NotBeNull();

        _orderRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Order>(o =>
            o.Id == orderId && 
            o.Status == OrderStatus.Completed
        )), Times.Once);
    }
}
