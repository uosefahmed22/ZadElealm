using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class ProgressConfiguration : IEntityTypeConfiguration<Progress>
{
    public void Configure(EntityTypeBuilder<Progress> builder)
    {
        builder.HasIndex(progress => new { progress.AppUserId, progress.QuizId })
            .IsUnique()
            .HasDatabaseName("UX_Progresses_AppUserId_QuizId")
            .HasFilter("[IsDeleted] = 0");
    }
}
