namespace JarconiRestaurant.DTOs.Menu;

public class MenuVm {
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    public int SortOrder { get; set; }
    public IEnumerable<MenuItemVm> Items { get; set; } = Enumerable.Empty<MenuItemVm>();
}