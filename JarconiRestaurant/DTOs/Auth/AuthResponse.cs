namespace JarconiRestaurant.DTOs.Auth;

public class AuthResponse {
    public string Token { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public int UserId { get; set; }
}

