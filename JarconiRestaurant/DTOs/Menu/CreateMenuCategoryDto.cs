namespace JarconiRestaurant.DTOs.Menu;

public class CreateMenuCategoryDto {
    public string Name { get; set; } = default!;
    public int SortOrder { get; set; } = 0;
}