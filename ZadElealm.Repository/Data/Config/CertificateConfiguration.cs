using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasIndex(certificate => new { certificate.UserId, certificate.QuizId })
            .IsUnique()
            .HasDatabaseName("UX_Certificates_UserId_QuizId")
            .HasFilter("[IsDeleted] = 0 AND [QuizId] IS NOT NULL");

        builder.HasIndex(certificate => new { certificate.UserId, certificate.AssessmentId })
            .IsUnique()
            .HasDatabaseName("UX_Certificates_UserId_AssessmentId")
            .HasFilter("[IsDeleted] = 0 AND [AssessmentId] IS NOT NULL");

        builder.HasOne(certificate => certificate.Assessment)
            .WithMany(assessment => assessment.Certificates)
            .HasForeignKey(certificate => certificate.AssessmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(certificate => certificate.Quiz)
            .WithMany(quiz => quiz.Certificates)
            .HasForeignKey(certificate => certificate.QuizId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
