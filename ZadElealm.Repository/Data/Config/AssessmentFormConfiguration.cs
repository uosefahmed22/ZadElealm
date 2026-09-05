using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class AssessmentFormConfiguration : IEntityTypeConfiguration<AssessmentForm>
{
    public void Configure(EntityTypeBuilder<AssessmentForm> builder)
    {
        builder.Property(x => x.InternalCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.AssessmentId, x.InternalCode })
            .IsUnique()
            .HasDatabaseName("UX_AssessmentForms_AssessmentId_InternalCode")
            .HasFilter("[IsDeleted] = 0");

        builder.HasMany(x => x.Questions)
            .WithOne(x => x.AssessmentForm)
            .HasForeignKey(x => x.AssessmentFormId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
