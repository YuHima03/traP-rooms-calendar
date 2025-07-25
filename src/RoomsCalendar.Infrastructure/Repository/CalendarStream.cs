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

        [Column("user_id")]
        public Guid UserId { get; set; }

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
                stream.UserId,
                stream.Token,
                stream.CreatedAt,
                stream.UpdatedAt
            );
        }
    }
}
