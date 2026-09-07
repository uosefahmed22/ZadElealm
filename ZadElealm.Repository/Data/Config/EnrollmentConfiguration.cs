using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasIndex(enrollment => new { enrollment.AppUserId, enrollment.CourseId })
            .IsUnique()
            .HasDatabaseName("UX_Enrollments_AppUserId_CourseId")
            .HasFilter("[IsDeleted] = 0");
    }
}
