using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class AssessmentChoiceConfiguration : IEntityTypeConfiguration<AssessmentChoice>
{
    public void Configure(EntityTypeBuilder<AssessmentChoice> builder)
    {
        builder.Property(x => x.Text).HasMaxLength(500).IsRequired();
    }
}
