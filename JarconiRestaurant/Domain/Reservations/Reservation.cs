using JarconiRestaurant.Domain.Common;
using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Users;

namespace JarconiRestaurant.Domain.Reservations;

public class Reservation : BaseEntity {
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int TableNumber { get; set; }
    public DateTime DateTimeStartUtc { get; set; }
    public int DurationMin { get; set; } = 90;
    public int PartySize { get; set; } = 1;
    public string? Comment { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
}