namespace RoomsCalendar.Server.Handlers
{
    public class Handler
    {
        readonly EventsHandler _eventsHandler = new();
        readonly RoomsHandler _roomsHandler = new();

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            var apiGroup = builder.MapGroup("api");
            _eventsHandler.MapHandlers(apiGroup);
            _roomsHandler.MapHandlers(apiGroup);
        }
    }
}
