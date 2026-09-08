using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.Property(achievement => achievement.UserId).IsRequired();
        builder.Property(achievement => achievement.Code).HasConversion<int>();
        builder.Property(achievement => achievement.UnlockedAtUtc).IsRequired();

        builder.HasIndex(achievement => new { achievement.UserId, achievement.Code })
            .IsUnique()
            .HasDatabaseName("UX_UserAchievements_UserId_Code")
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(achievement => achievement.User)
            .WithMany()
            .HasForeignKey(achievement => achievement.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
