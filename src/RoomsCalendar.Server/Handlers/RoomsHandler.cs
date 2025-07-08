using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Handlers
{
    public class RoomsHandler
    {
        [HttpGet]
        [ProducesResponseType<Room[]>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        async ValueTask<Results<Ok<Room[]>, BadRequest<string>>> GetRoomsAsync(
            HttpContext ctx,
            [FromServices] IRoomsProvider roomsProvider,
            [FromQuery(Name = "since")] DateTimeOffset? since = null,
            [FromQuery(Name = "until")] DateTimeOffset? until = null)
        {
            if (since > until)
            {
                return BadRequest_InvalidTime;
            }
            var ct = ctx.RequestAborted;
            var rooms = await roomsProvider.GetRoomsAsync(since ?? DateTimeOffset.MinValue, until ?? DateTimeOffset.MaxValue, ct);
            return TypedResults.Ok(rooms);
        }

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            builder.MapGet("rooms", GetRoomsAsync);
        }

        static readonly BadRequest<string> BadRequest_InvalidTime = TypedResults.BadRequest("""
            {"message":"The 'since' parameter must be less than or equal to the 'until' parameter."}
            """);
    }
}
