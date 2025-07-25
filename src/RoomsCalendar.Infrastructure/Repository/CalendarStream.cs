using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomsCalendar.Infrastructure.Repository
{
    [Table("calendar_streams")]
    sealed class CalendarStream
    {
        [Column("id")]
        [Key]
        public Guid Id { get; set; }

        [Column("username")]
        public required string Username { get; set; }

        [Column("token")]
        public required string Token { get; set; }

        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("updated_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    static class CalendarStreamExtensions
    {
        public static Share.Domain.CalendarStream ToDomain(this CalendarStream stream)
        {
            return new Share.Domain.CalendarStream(
                stream.Id,
                stream.Username,
                stream.Token,
                stream.CreatedAt,
                stream.UpdatedAt
            );
        }
    }
}
