using JarconiRestaurant.Domain.Enums;
using JarconiRestaurant.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JarconiRestaurant.Data.Configurations;

public class ReservationConfig : IEntityTypeConfiguration<Reservation> {

    public void Configure(EntityTypeBuilder<Reservation> b) {
        b.ToTable("reservations");

        b.Property(x => x.TableNumber).IsRequired();
        b.Property(x => x.DateTimeStartUtc).IsRequired();

        b.Property(x => x.DurationMin).HasDefaultValue(90);
        b.Property(x => x.PartySize).HasDefaultValue(1);

        b.Property(x => x.Status)
            .HasConversion<int>()
            .HasDefaultValue((int)ReservationStatus.Pending);

        b.Property(x => x.CreatedAtUtc)
            .HasDefaultValueSql("timezone('utc', now())");

        // Индексы
        b.HasIndex(x => x.DateTimeStartUtc);
        b.HasIndex(x => new { x.TableNumber, x.DateTimeStartUtc });
        b.HasIndex(x => new { x.TableNumber, x.DateTimeStartUtc, x.Status })
            .HasDatabaseName("IX_Reservations_Table_Time_Status")
            .HasFilter("\"Status\" IN (1,2)"); // Pending, Confirmed

        // Связь с пользователем
        b.HasOne(x => x.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Check constraints
        b.HasCheckConstraint("CK_Reservation_Table_Positive", "\"TableNumber\" > 0");
        b.HasCheckConstraint("CK_Reservation_Party_Positive", "\"PartySize\" > 0");
        b.HasCheckConstraint("CK_Reservation_Duration_Allowed", "\"DurationMin\" IN (60,90,120)");
    }
}