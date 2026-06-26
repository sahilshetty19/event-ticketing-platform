using Microsoft.AspNetCore.Mvc;
using Notification.Application.Abstractions;
using Notification.Application.Dtos;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService) =>
        _notificationService = notificationService;

    /// <summary>Lists recently sent notifications (useful for verifying the end-to-end flow).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SentNotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SentNotificationDto>>> GetRecent(CancellationToken ct)
        => Ok(await _notificationService.GetRecentAsync(50, ct));
}
