namespace RoomsCalendar.Server.Handlers
{
    public class Handler
    {
        readonly EventsHandler _eventsHandler = new();
        readonly RoomsHandler _roomsHandler = new();
        readonly RoomsIcalHandler _roomsIcalHandler = new();

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            var apiGroup = builder.MapGroup("api");
            _eventsHandler.MapHandlers(apiGroup);
            _roomsHandler.MapHandlers(apiGroup);
            _roomsIcalHandler.MapHandlers(apiGroup);
        }
    }
}
