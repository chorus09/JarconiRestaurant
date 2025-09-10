using JarconiRestaurant.Domain.Common;

namespace JarconiRestaurant.Domain.Menu;

public class MenuItem : BaseEntity {
    public int CategoryId { get; set; }
    public MenuCategory Category { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}