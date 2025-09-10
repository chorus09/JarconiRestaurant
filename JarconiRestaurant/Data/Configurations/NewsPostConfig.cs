using JarconiRestaurant.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JarconiRestaurant.Data.Configurations;

public class NewsPostConfig : IEntityTypeConfiguration<NewsPost> {

    public void Configure(EntityTypeBuilder<NewsPost> b) {
        b.ToTable("news_posts");

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Slug).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.Slug).IsUnique();

        b.Property(x => x.Excerpt).HasMaxLength(500);
        b.Property(x => x.Content).IsRequired();

        b.Property(x => x.IsPublished).HasDefaultValue(false);
        b.Property(x => x.PublishedAtUtc).HasDefaultValue(null);

        b.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");

        b.HasIndex(x => new { x.IsPublished, x.PublishedAtUtc });
    }
}