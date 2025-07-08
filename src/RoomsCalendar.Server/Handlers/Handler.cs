namespace RoomsCalendar.Server.Handlers
{
    public class Handler
    {
        readonly RoomsHandler _roomsHandler = new();

        public void MapHandlers(IEndpointRouteBuilder builder)
        {
            var apiGroup = builder.MapGroup("api");
            _roomsHandler.MapHandlers(apiGroup);
        }
    }
}
