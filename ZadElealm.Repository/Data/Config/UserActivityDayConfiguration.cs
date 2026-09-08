using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class UserActivityDayConfiguration : IEntityTypeConfiguration<UserActivityDay>
{
    public void Configure(EntityTypeBuilder<UserActivityDay> builder)
    {
        builder.Property(activity => activity.UserId).IsRequired();
        builder.Property(activity => activity.ActivityDate).HasColumnType("date").IsRequired();

        builder.HasIndex(activity => new { activity.UserId, activity.ActivityDate })
            .IsUnique()
            .HasDatabaseName("UX_UserActivityDays_UserId_ActivityDate")
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(activity => activity.User)
            .WithMany()
            .HasForeignKey(activity => activity.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
