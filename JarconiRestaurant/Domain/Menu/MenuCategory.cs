using JarconiRestaurant.Domain.Common;

namespace JarconiRestaurant.Domain.Menu;

public class MenuCategory : BaseEntity {
    public string Name { get; set; } = default!;
    public int SortOrder { get; set; } = 0;
    public bool IsPublished { get; set; } = true;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}