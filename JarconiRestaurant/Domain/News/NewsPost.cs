using JarconiRestaurant.Domain.Common;

namespace JarconiRestaurant.Domain.News;

public class NewsPost : BaseEntity {
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}