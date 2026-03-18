using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotosMarket.API.Services;
using PhotosMarket.API.DTOs;
using System.Security.Claims;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            var order = await _orderService.CreateOrderAsync(userId, userEmail, request.Photos);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt
            };

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = orderDto,
                Message = "Order created successfully. Please proceed with payment."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to create order",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrders()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var orders = await _orderService.GetUserOrdersAsync(userId);

            var orderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            }).ToList();

            return Ok(new ApiResponse<List<OrderDto>>
            {
                Success = true,
                Data = orderDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching orders");
            return BadRequest(new ApiResponse<List<OrderDto>>
            {
                Success = false,
                Message = "Failed to fetch orders",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            if (order == null)
                return NotFound();

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            };

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = orderDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order {OrderId}", orderId);
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to fetch order",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{orderId}/confirm-payment")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> ConfirmPayment(string orderId, [FromBody] ConfirmPaymentRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var order = await _orderService.ConfirmPaymentAsync(orderId, userId);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            };

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = orderDto,
                Message = "Payment confirmed. Your order is now being prepared."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment for order {OrderId}", orderId);
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to confirm payment",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{orderId}/complete")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CompleteOrder(string orderId)
    {
        try
        {
            var order = await _orderService.CompleteOrderAsync(orderId);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                ProcessedAt = order.ProcessedAt
            };

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = orderDto,
                Message = "Order completed successfully. Download link sent to customer."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing order {OrderId}", orderId);
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to complete order",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{orderId}/cancel")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CancelOrder(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            var order = await _orderService.CancelOrderAsync(orderId, userId, isAdmin);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            };

            return Ok(new ApiResponse<OrderDto>
            {
                Success = true,
                Data = orderDto,
                Message = "Order cancelled successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
            return BadRequest(new ApiResponse<OrderDto>
            {
                Success = false,
                Message = "Failed to cancel order",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();

            var orderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt
            }).ToList();

            return Ok(new ApiResponse<List<OrderDto>>
            {
                Success = true,
                Data = orderDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all orders");
            return BadRequest(new ApiResponse<List<OrderDto>>
            {
                Success = false,
                Message = "Failed to fetch orders",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
