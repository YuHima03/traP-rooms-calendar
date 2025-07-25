using Microsoft.EntityFrameworkCore;
using RoomsCalendar.Share.Domain.Repository;

namespace RoomsCalendar.Infrastructure.Repository
{
    sealed class CalendarStreamsRepository(DbContextOptions<CalendarStreamsRepository> options) : DbContext(options), ICalendarStreamsRepository
    {
        DbSet<CalendarStream> CalendarStreams { get; set; }

        public async ValueTask<Share.Domain.CalendarStream> GetCalendarStreamAsync(Guid streamId, CancellationToken ct = default)
        {
            return await CalendarStreams
                .AsNoTracking()
                .Where(cs => cs.Id == streamId)
                .Select(cs => cs.ToDomain())
                .SingleAsync(ct);
        }

        public async ValueTask<Share.Domain.CalendarStream> GetOrCreateUserCalendarStreamAsync(Guid userId, CancellationToken ct = default)
        {
            var cs = await CalendarStreams
                .AsNoTracking()
                .Where(cs => cs.UserId == userId)
                .Select(cs => cs.ToDomain())
                .SingleOrDefaultAsync(ct);
            if (cs is not null)
            {
                return cs;
            }
            CalendarStream rec = new()
            {
                Id = Guid.CreateVersion7(),
                UserId = userId,
                Token = GenerateToken()
            };
            CalendarStreams.Add(rec);
            await SaveChangesAsync(ct);
            return rec.ToDomain();
        }

        public async ValueTask<Share.Domain.CalendarStream> RefreshCalendarStreamTokenAsync(Guid streamId, CancellationToken ct = default)
        {
            var cs = await CalendarStreams
                .Where(cs => cs.Id == streamId)
                .SingleAsync(ct);
            cs.Token = GenerateToken();
            await SaveChangesAsync(ct);
            return cs.ToDomain();
        }

        const string TokenChars = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
        const int TokenLength = 32;

        static string GenerateToken()
        {
            Span<char> token = stackalloc char[TokenLength];
            Random.Shared.GetItems(TokenChars.AsSpan(), token);
            return token.ToString();
        }
    }
}
