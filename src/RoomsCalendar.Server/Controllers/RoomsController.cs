using Microsoft.AspNetCore.Mvc;
using RoomsCalendar.Server.Domain;

namespace RoomsCalendar.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> IndexAsync(
            [FromServices] IRoomsProvider roomsProvider,
            [FromQuery(Name = "since")] DateTimeOffset? since = null,
            [FromQuery(Name = "until")] DateTimeOffset? until = null)
        {
            if (since > until)
            {
                return BadRequest("The 'since' parameter must be less than or equal to the 'until' parameter.");
            }
            var ct = HttpContext.RequestAborted;
            var rooms = await roomsProvider.GetRoomsAsync(since ?? DateTimeOffset.MinValue, until ?? DateTimeOffset.MaxValue, ct);
            return Ok(rooms);
        }
    }
}
