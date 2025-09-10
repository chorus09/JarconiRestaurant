using JarconiRestaurant.Domain.Menu;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JarconiRestaurant.Data.Configurations;

public class MenuCategoryConfig : IEntityTypeConfiguration<MenuCategory> {

    public void Configure(EntityTypeBuilder<MenuCategory> b) {
        b.ToTable("menu_categories");

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(x => x.Name).IsUnique();

        b.Property(x => x.SortOrder).HasDefaultValue(0);
        b.Property(x => x.IsPublished).HasDefaultValue(true);

        b.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");
    }
}