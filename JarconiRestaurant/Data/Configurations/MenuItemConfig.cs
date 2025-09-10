using JarconiRestaurant.Domain.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JarconiRestaurant.Data.Configurations;

public class MenuItemConfig : IEntityTypeConfiguration<MenuItem> {

    public void Configure(EntityTypeBuilder<MenuItem> b) {
        b.ToTable("menu_items");

        b.Property(x => x.Title).IsRequired().HasMaxLength(150);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Price).HasPrecision(10, 2);
        b.Property(x => x.IsAvailable).HasDefaultValue(true);
        b.Property(x => x.IsPublished).HasDefaultValue(true);
        b.Property(x => x.SortOrder).HasDefaultValue(0);

        b.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");

        b.HasOne(x => x.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Уникальный Title в рамках категории
        b.HasIndex(x => new { x.CategoryId, x.Title }).IsUnique();

        b.HasCheckConstraint("CK_MenuItem_Price_Positive", "\"Price\" > 0");
    }
}