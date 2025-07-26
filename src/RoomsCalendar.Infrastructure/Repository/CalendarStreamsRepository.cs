using Microsoft.EntityFrameworkCore;
using RoomsCalendar.Share.Domain.Repository;

namespace RoomsCalendar.Infrastructure.Repository
{
    public sealed class CalendarStreamsRepository(DbContextOptions<CalendarStreamsRepository> options) : DbContext(options), ICalendarStreamsRepository
    {
        DbSet<CalendarStream> CalendarStreams { get; set; }

        async ValueTask<Share.Domain.CalendarStream?> ICalendarStreamsRepository.TryGetCalendarStreamAsync(Guid streamId, CancellationToken ct)
        {
            return await CalendarStreams
                .AsNoTracking()
                .Where(cs => cs.Id == streamId)
                .Select(cs => cs.ToDomain())
                .SingleOrDefaultAsync(ct);
        }

        async ValueTask<Share.Domain.CalendarStream> ICalendarStreamsRepository.GetOrCreateUserCalendarStreamAsync(string username, CancellationToken ct)
        {
            var cs = await CalendarStreams
                .AsNoTracking()
                .Where(cs => cs.Username == username)
                .Select(cs => cs.ToDomain())
                .SingleOrDefaultAsync(ct);
            if (cs is not null)
            {
                return cs;
            }
            CalendarStream rec = new()
            {
                Id = Guid.CreateVersion7(),
                Username = username,
                Token = GenerateToken()
            };
            CalendarStreams.Add(rec);
            await SaveChangesAsync(ct);
            return rec.ToDomain();
        }

        async ValueTask<Share.Domain.CalendarStream?> ICalendarStreamsRepository.TryRefreshCalendarStreamTokenAsync(Guid streamId, CancellationToken ct)
        {
            var cs = await CalendarStreams
                .Where(cs => cs.Id == streamId)
                .SingleOrDefaultAsync(ct);
            if (cs is null)
            {
                return null;
            }
            cs.Token = GenerateToken();
            await SaveChangesAsync(ct);
            return cs.ToDomain();
        }

        const string TokenChars = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
        const int TokenLength = 16;

        static string GenerateToken()
        {
            Span<char> token = stackalloc char[TokenLength];
            Random.Shared.GetItems(TokenChars.AsSpan(), token);
            return token.ToString();
        }
    }
}
