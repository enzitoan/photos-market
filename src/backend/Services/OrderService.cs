using Microsoft.Extensions.Options;
using PhotosMarket.API.Configuration;
using PhotosMarket.API.Models;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.DTOs;

namespace PhotosMarket.API.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(string userId, string userEmail, List<OrderPhotoDto> photos);
    Task<Order?> GetOrderByIdAsync(string orderId, string userId);
    Task<List<Order>> GetUserOrdersAsync(string userId);
    Task<List<Order>> GetAllOrdersAsync();
    Task<Order> ConfirmPaymentAsync(string orderId, string userId);
    Task<Order> CompleteOrderAsync(string orderId);
    Task<Order> CancelOrderAsync(string orderId, string userId, bool isAdmin = false);
    Task<DownloadLink> GenerateDownloadLinkAsync(Order order);
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDownloadLinkRepository _downloadLinkRepository;
    private readonly IEmailService _emailService;
    private readonly IPhotographerSettingsRepository _photographerSettingsRepository;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IDownloadLinkRepository downloadLinkRepository,
        IEmailService emailService,
        IPhotographerSettingsRepository photographerSettingsRepository,
        IOptions<ApplicationSettings> appSettings,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _downloadLinkRepository = downloadLinkRepository;
        _emailService = emailService;
        _photographerSettingsRepository = photographerSettingsRepository;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(string userId, string userEmail, List<OrderPhotoDto> photos)
    {
        // Get current photographer settings for price and currency
        var settings = await _photographerSettingsRepository.GetSettingsAsync();
        var photoPrice = settings?.PhotoPrice ?? _appSettings.PhotoPricePerUnit;
        var currency = settings?.Currency ?? _appSettings.Currency;

        var order = new Order
        {
            UserId = userId,
            UserEmail = userEmail,
            Photos = photos.Select(p => new OrderPhoto
            {
                PhotoId = p.PhotoId,
                MediaItemId = p.MediaItemId,
                Filename = p.Filename,
                BaseUrl = p.BaseUrl,
                Price = photoPrice,
                AlbumId = p.AlbumId,
                AlbumTitle = p.AlbumTitle
            }).ToList(),
            Currency = currency,
            Status = OrderStatus.AwaitingPayment,
            CreatedAt = DateTime.UtcNow
        };

        order.TotalAmount = order.Photos.Sum(p => p.Price);

        order = await _orderRepository.CreateAsync(order);

        // Send order confirmation email (non-blocking)
        try
        {
            await _emailService.SendOrderConfirmationEmailAsync(order, userEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send order confirmation email for order {OrderId}. Order was created successfully.", order.Id);
            // Continue - don't fail order creation if email fails
        }

        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(string orderId, string userId)
    {
        return await _orderRepository.GetByIdAsync(orderId, userId);
    }

    public async Task<List<Order>> GetUserOrdersAsync(string userId)
    {
        return await _orderRepository.GetByUserIdAsync(userId);
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        return await _orderRepository.GetAllAsync();
    }

    public async Task<Order> ConfirmPaymentAsync(string orderId, string userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, userId);
        
        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (order.Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException("Order is not awaiting payment");

        order.Status = OrderStatus.Processing;
        order.PaidAt = DateTime.UtcNow;

        order = await _orderRepository.UpdateAsync(order);

        return order;
    }

    public async Task<Order> CompleteOrderAsync(string orderId)
    {
        var order = await _orderRepository.GetAllAsync();
        var orderToComplete = order.FirstOrDefault(o => o.Id == orderId);
        
        if (orderToComplete == null)
            throw new InvalidOperationException("Order not found");

        if (orderToComplete.Status != OrderStatus.Processing)
            throw new InvalidOperationException("Order must be in Processing status to be completed");

        // Generate download link
        var downloadLink = await GenerateDownloadLinkAsync(orderToComplete);

        // Update order status to completed
        orderToComplete.Status = OrderStatus.Completed;
        orderToComplete.ProcessedAt = DateTime.UtcNow;
        orderToComplete = await _orderRepository.UpdateAsync(orderToComplete);

        // Send download link email (non-blocking)
        var downloadUrl = $"{_appSettings.FrontendUrl}/download/{downloadLink.Token}";
        try
        {
            await _emailService.SendDownloadLinkEmailAsync(orderToComplete.UserEmail, downloadUrl, downloadLink.ExpiresAt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send download link email for order {OrderId}. Download link was created successfully.", orderToComplete.Id);
            // Continue - don't fail completion if email fails
        }

        return orderToComplete;
    }

    public async Task<Order> CancelOrderAsync(string orderId, string userId, bool isAdmin = false)
    {
        Order? order;
        
        if (isAdmin)
        {
            var allOrders = await _orderRepository.GetAllAsync();
            order = allOrders.FirstOrDefault(o => o.Id == orderId);
        }
        else
        {
            order = await _orderRepository.GetByIdAsync(orderId, userId);
        }
        
        if (order == null)
            throw new InvalidOperationException("Order not found");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed order");

        order.Status = OrderStatus.Cancelled;
        order = await _orderRepository.UpdateAsync(order);

        return order;
    }

    public async Task<DownloadLink> GenerateDownloadLinkAsync(Order order)
    {
        // Check if download link already exists
        var existingLink = await _downloadLinkRepository.GetByOrderIdAsync(order.Id, order.UserId);
        if (existingLink != null && !existingLink.IsExpired)
        {
            return existingLink;
        }

        var downloadLink = new DownloadLink
        {
            OrderId = order.Id,
            UserId = order.UserId,
            PhotoUrls = order.Photos.Select(p => $"{p.BaseUrl}=d").ToList(), // =d parameter for download
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_appSettings.DownloadLinkExpirationHours)
        };

        return await _downloadLinkRepository.CreateAsync(downloadLink);
    }
}
