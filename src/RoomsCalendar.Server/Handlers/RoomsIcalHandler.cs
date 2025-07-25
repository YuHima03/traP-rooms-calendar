using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomsCalendar.Infrastructure.Repository;
using RoomsCalendar.Server.Services;
using RoomsCalendar.Share.Domain.Repository;
using System.Text;

namespace RoomsCalendar.Server.Handlers
{
    sealed class RoomsIcalHandler
    {
        [HttpGet]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        async ValueTask<IResult> GetRoomsIcalAsync(
            HttpContext ctx,
            [FromServices] RoomsCalendarProvider calendarProvider,
            [FromServices] IDbContextFactory<CalendarStreamsRepository> repoFactory,
            [FromRoute] Guid id,
            [FromRoute] string token,
            [FromQuery(Name = "excludeOccupied")] bool excludeOccupied = false)
        {
            var ct = ctx.RequestAborted;
            try
            {
                await using var repo = (await repoFactory.CreateDbContextAsync(ct)) as ICalendarStreamsRepository;
                var cs = await repo.TryGetCalendarStreamAsync(id, ct);
                if (cs is null || cs.Token != token)
                {
                    return TypedResults.NotFound();
                }
                return Results.Text(
                    await calendarProvider.GetIcalStringAsync(excludeOccupied, ct),
                    "text/calendar",
                    Encoding.UTF8
                );
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
            builder.MapGet("rooms/ical/{id:guid:required}/{token}", GetRoomsIcalAsync)
                .AllowAnonymous();
        }
    }
}
