using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class AssessmentProgressConfiguration : IEntityTypeConfiguration<AssessmentProgress>
{
    public void Configure(EntityTypeBuilder<AssessmentProgress> builder)
    {
        builder.HasIndex(x => new { x.AppUserId, x.AssessmentId })
            .IsUnique()
            .HasDatabaseName("UX_AssessmentProgresses_AppUserId_AssessmentId")
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(x => x.Assessment)
            .WithMany(x => x.Progresses)
            .HasForeignKey(x => x.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssessmentForm)
            .WithMany(x => x.Progresses)
            .HasForeignKey(x => x.AssessmentFormId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
