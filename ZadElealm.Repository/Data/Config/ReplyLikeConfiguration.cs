using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config
{
    public class ReplyLikeConfiguration : IEntityTypeConfiguration<ReplyLike>
    {
        public void Configure(EntityTypeBuilder<ReplyLike> builder)
        {
            builder.HasIndex(x => new { x.AppUserId, x.ReplyId })
                .IsUnique()
                .HasDatabaseName("UX_ReplyLike_AppUserId_ReplyId");

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
