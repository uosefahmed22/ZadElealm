using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class AssessmentQuestionConfiguration : IEntityTypeConfiguration<AssessmentQuestion>
{
    public void Configure(EntityTypeBuilder<AssessmentQuestion> builder)
    {
        builder.Property(x => x.Text).HasMaxLength(1000).IsRequired();
        builder.HasMany(x => x.Choices)
            .WithOne(x => x.AssessmentQuestion)
            .HasForeignKey(x => x.AssessmentQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
