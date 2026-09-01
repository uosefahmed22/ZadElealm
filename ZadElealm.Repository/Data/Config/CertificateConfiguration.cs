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
            .HasFilter("[IsDeleted] = 0");
    }
}
