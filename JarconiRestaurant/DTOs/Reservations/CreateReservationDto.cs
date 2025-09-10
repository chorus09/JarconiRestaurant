namespace JarconiRestaurant.DTOs.Reservations;

public class CreateReservationDto {
    public int UserId { get; set; }                 // временно, до JWT
    public int TableNumber { get; set; }
    public DateTime DateTimeStartUtc { get; set; }  // ISO-8601 UTC
    public int DurationMin { get; set; } = 90;
    public int PartySize { get; set; } = 1;
    public string? Comment { get; set; }
}