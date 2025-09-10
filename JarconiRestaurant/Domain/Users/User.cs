using JarconiRestaurant.Domain.Common;
using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Reservations;

namespace JarconiRestaurant.Domain.Users;

public class User : BaseEntity {
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public UserRole Role { get; set; }
    public bool IsBlocked { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}