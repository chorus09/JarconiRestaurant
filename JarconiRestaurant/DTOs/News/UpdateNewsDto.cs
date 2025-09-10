namespace JarconiRestaurant.DTOs.News;

public class UpdateNewsDto {
    public string Title { get; set; } = default!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = default!;
}