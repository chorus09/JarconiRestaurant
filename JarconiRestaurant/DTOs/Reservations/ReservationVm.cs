using JarconiRestaurant.Domain.Enums;

namespace JarconiRestaurant.DTOs.Reservations;

public class ReservationVm {
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TableNumber { get; set; }
    public DateTime DateTimeStartUtc { get; set; }
    public int DurationMin { get; set; }
    public int PartySize { get; set; }
    public string? Comment { get; set; }
    public ReservationStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}