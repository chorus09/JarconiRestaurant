namespace JarconiRestaurant.DTOs.News;

public class CreateNewsDto {
    public string Title { get; set; } = default!;
    public string? Slug { get; set; }            // если не передали — сгенерим
    public string? Excerpt { get; set; }
    public string Content { get; set; } = default!;
}