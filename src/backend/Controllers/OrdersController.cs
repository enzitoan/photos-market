using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotosMarket.API.Services;
using PhotosMarket.API.DTOs;
using PhotosMarket.API.Models;
using PhotosMarket.API.Repositories;
using PhotosMarket.API.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace PhotosMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IDownloadLinkRepository _downloadLinkRepository;
    private readonly ApplicationSettings _appSettings;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderService orderService,
        IDownloadLinkRepository downloadLinkRepository,
        IOptions<ApplicationSettings> appSettings,
        ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _downloadLinkRepository = downloadLinkRepository;
        _appSettings = appSettings.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? request.UserName;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userEmail))
                return Unauthorized();

            // Si no hay userName en los claims ni en la request, usar el email
            if (string.IsNullOrEmpty(userName))
                userName = userEmail.Split('@')[0];

            var order = await _orderService.CreateOrderAsync(userId, userEmail, userName, request.Photos);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaymentReference = order.PaymentReference
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
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                PaymentReference = order.PaymentReference
            })
            .OrderByDescending(o => o.CreatedAt)
            .ToList();

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
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                PaymentReference = order.PaymentReference
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

            var isAdmin = User.IsInRole("Admin");
            var order = await _orderService.ConfirmPaymentAsync(orderId, userId, request.PaymentReference, isAdmin);

            var orderDto = new OrderDto
            {
                Id = order.Id,
                UserEmail = order.UserEmail,
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                PaymentReference = order.PaymentReference
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
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                ProcessedAt = order.ProcessedAt,
                PaymentReference = order.PaymentReference
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
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                PaymentReference = order.PaymentReference
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
                UserName = order.UserName,
                Photos = order.Photos.Select(p => new OrderPhotoDto
                {
                    PhotoId = p.PhotoId,
                    MediaItemId = p.MediaItemId,
                    Filename = p.Filename,
                    BaseUrl = p.BaseUrl,
                    AlbumId = p.AlbumId,
                    AlbumTitle = p.AlbumTitle
                }).ToList(),
                Subtotal = order.Subtotal,
                DiscountPercentage = order.DiscountPercentage,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status.ToString(),
                CreatedAt = order.CreatedAt,
                PaidAt = order.PaidAt,
                PaymentReference = order.PaymentReference
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

    [HttpGet("{orderId}/download-link")]
    public async Task<ActionResult<ApiResponse<DownloadLinkDto>>> GetOrderDownloadLink(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Verificar que el pedido pertenezca al usuario
            var order = await _orderService.GetOrderByIdAsync(orderId, userId);
            if (order == null)
                return NotFound(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Order not found"
                });

            // Buscar el link de descarga
            var downloadLink = await _downloadLinkRepository.GetByOrderIdAsync(orderId, userId);
            
            if (downloadLink == null)
            {
                return NotFound(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Download link not found for this order"
                });
            }

            // Verificar si el link está expirado
            if (downloadLink.IsExpired)
            {
                return Ok(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Download link has expired",
                    Data = new DownloadLinkDto
                    {
                        Token = downloadLink.Token,
                        ExpiresAt = downloadLink.ExpiresAt,
                        IsExpired = true
                    }
                });
            }

            // Generar URL absoluta para descarga
            var baseUrl = !string.IsNullOrEmpty(_appSettings.BaseUrl) 
                ? _appSettings.BaseUrl 
                : $"{Request.Scheme}://{Request.Host}";
            var downloadUrl = $"{baseUrl}/api/download/{downloadLink.Token}";

            var downloadLinkDto = new DownloadLinkDto
            {
                Token = downloadLink.Token,
                DownloadUrl = downloadUrl,
                ExpiresAt = downloadLink.ExpiresAt,
                IsExpired = downloadLink.IsExpired,
                OrderNumber = order.Id.Substring(0, 8).ToUpper(),
                CustomerName = order.UserName,
                CustomerEmail = order.UserEmail
            };

            return Ok(new ApiResponse<DownloadLinkDto>
            {
                Success = true,
                Data = downloadLinkDto,
                Message = "Download link retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching download link for order {OrderId}", orderId);
            return BadRequest(new ApiResponse<DownloadLinkDto>
            {
                Success = false,
                Message = "Failed to fetch download link",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{orderId}/regenerate-download-link")]
    public async Task<ActionResult<ApiResponse<DownloadLinkDto>>> RegenerateDownloadLink(string orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            
            // Verificar que el pedido pertenezca al usuario o sea admin
            Order? order;
            if (isAdmin)
            {
                var allOrders = await _orderService.GetAllOrdersAsync();
                order = allOrders.FirstOrDefault(o => o.Id == orderId);
            }
            else
            {
                order = await _orderService.GetOrderByIdAsync(orderId, userId);
            }
            
            if (order == null)
                return NotFound(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Order not found"
                });

            // Verificar que el pedido esté completado
            if (order.Status != OrderStatus.Completed)
            {
                return BadRequest(new ApiResponse<DownloadLinkDto>
                {
                    Success = false,
                    Message = "Order must be completed to generate download link"
                });
            }

            // Generar nuevo link de descarga
            var downloadLink = await _orderService.GenerateDownloadLinkAsync(order);

            // Generar URL absoluta
            var baseUrl = !string.IsNullOrEmpty(_appSettings.BaseUrl) 
                ? _appSettings.BaseUrl 
                : $"{Request.Scheme}://{Request.Host}";
            var downloadUrl = $"{baseUrl}/api/download/{downloadLink.Token}";

            var downloadLinkDto = new DownloadLinkDto
            {
                Token = downloadLink.Token,
                DownloadUrl = downloadUrl,
                ExpiresAt = downloadLink.ExpiresAt,
                IsExpired = downloadLink.IsExpired,
                OrderNumber = order.Id.Substring(0, 8).ToUpper(),
                CustomerName = order.UserName,
                CustomerEmail = order.UserEmail
            };

            return Ok(new ApiResponse<DownloadLinkDto>
            {
                Success = true,
                Data = downloadLinkDto,
                Message = "Download link generated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating download link for order {OrderId}", orderId);
            return BadRequest(new ApiResponse<DownloadLinkDto>
            {
                Success = false,
                Message = "Failed to regenerate download link",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
