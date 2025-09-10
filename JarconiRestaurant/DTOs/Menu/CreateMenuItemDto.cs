namespace JarconiRestaurant.DTOs.Menu;

public class CreateMenuItemDto {
    public int CategoryId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsPublished { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}