using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RoomsCalendar.Server.Services;

namespace RoomsCalendar.Server.Handlers
{
    sealed class RoomsIcalHandler
    {
        [HttpGet]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        async ValueTask<Results<Ok<string>, BadRequest<string>>> GetRoomsIcalAsync(
            HttpContext ctx,
            [FromServices] RoomsCalendarProvider calendarProvider,
            [FromRoute] Guid id,
            [FromRoute] string token,
            [FromQuery(Name = "excludeOccupied")] bool excludeOccupied = false)
        {
            var ct = ctx.RequestAborted;
            try
            {
                ctx.Response.Headers.ContentType = "text/calendar";
                return TypedResults.Ok(await calendarProvider.GetIcalStringAsync(excludeOccupied, ct));
            }
            catch (NotImplementedException)
            {
                ctx.Response.Headers.ContentType = "application/json";
                return TypedResults.BadRequest("""
                    {"message":"Ical generation for occupied rooms is not implemented."}
                    """);
            }
        }

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            builder.MapGroup("rooms/ical")
                .MapGet("{id:guid:required}/{token}", GetRoomsIcalAsync);
        }
    }
}
