﻿using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RoomsCalendar.Share.Domain;

namespace RoomsCalendar.Server.Handlers
{
    public class EventsHandler
    {
        [HttpGet]
        [ProducesResponseType<Event[]>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        async ValueTask<Results<Ok<Event[]>, BadRequest<string>>> GetEventsAsync(
            HttpContext ctx,
            [FromServices] IEventsProvider eventsProvider,
            [FromQuery(Name = "since")] DateTimeOffset? since = null,
            [FromQuery(Name = "until")] DateTimeOffset? until = null)
        {
            if (since > until)
            {
                return BadRequest_InvalidTime;
            }
            var ct = ctx.RequestAborted;
            var rooms = await eventsProvider.GetEventsAsync(since ?? DateTimeOffset.MinValue, until ?? DateTimeOffset.MaxValue, ct);
            return TypedResults.Ok(rooms);
        }

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            builder.MapGet("events", GetEventsAsync);
        }

        static readonly BadRequest<string> BadRequest_InvalidTime = TypedResults.BadRequest("""
            {"message":"The 'since' parameter must be less than or equal to the 'until' parameter."}
            """);
    }
}
