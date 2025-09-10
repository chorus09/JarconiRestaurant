using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JarconiRestaurant.Data.Configurations;

public class UserConfig : IEntityTypeConfiguration<User> {

    public void Configure(EntityTypeBuilder<User> b) {
        b.ToTable("users");

        b.Property(x => x.Email).IsRequired().HasMaxLength(254);
        b.HasIndex(x => x.Email).IsUnique();

        b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);

        b.Property(x => x.Role)
            .HasConversion<int>()
            .HasDefaultValue(UserRole.Client);

        b.Property(x => x.IsBlocked).HasDefaultValue(false);

        b.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");
    }
}