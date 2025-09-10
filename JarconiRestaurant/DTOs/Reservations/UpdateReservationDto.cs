namespace JarconiRestaurant.DTOs.Reservations;

public class UpdateReservationDto {
    public int TableNumber { get; set; }
    public DateTime DateTimeStartUtc { get; set; }
    public int DurationMin { get; set; } = 90;
    public int PartySize { get; set; } = 1;
    public string? Comment { get; set; }
}