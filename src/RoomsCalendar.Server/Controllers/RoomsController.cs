using Microsoft.AspNetCore.Mvc;
using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType<Room[]>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Room[]>> IndexAsync(
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
