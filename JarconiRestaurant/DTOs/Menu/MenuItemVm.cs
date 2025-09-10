namespace JarconiRestaurant.DTOs.Menu {

    public class MenuItemVm {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsPublished { get; set; }
        public int SortOrder { get; set; }
    }
}