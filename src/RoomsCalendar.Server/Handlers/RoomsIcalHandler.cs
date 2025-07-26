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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        async ValueTask<IResult> GetRoomsIcalAsync(
            HttpContext ctx,
            [FromServices] RoomsCalendarProvider calendarProvider,
            [FromServices] IDbContextFactory<CalendarStreamsRepository> repoFactory,
            [FromRoute] string id,
            [FromRoute] string token,
            [FromQuery(Name = "excludeOccupied")] bool excludeOccupied = false)
        {
            var ct = ctx.RequestAborted;
            try
            {
                Span<byte> guidBytes = stackalloc byte[16];
                if (!SimpleBase.Base58.Bitcoin.TryDecode(id, guidBytes, out var len) || len != guidBytes.Length)
                {
                    return TypedResults.NotFound();
                }
                Guid guid = new(guidBytes);

                await using var repo = (await repoFactory.CreateDbContextAsync(ct)) as ICalendarStreamsRepository;
                var cs = await repo.TryGetCalendarStreamAsync(guid, ct);
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
            builder.MapGet("rooms/ical/{id:required}/{token:required}", GetRoomsIcalAsync)
                .AllowAnonymous();
        }
    }
}
