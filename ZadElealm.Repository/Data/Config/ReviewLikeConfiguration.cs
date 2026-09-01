using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config
{
    public class ReviewLikeConfiguration : IEntityTypeConfiguration<ReviewLike>
    {
        public void Configure(EntityTypeBuilder<ReviewLike> builder)
        {
            builder.HasIndex(x => new { x.AppUserId, x.ReviewId })
                .IsUnique()
                .HasDatabaseName("UX_ReviewLikes_AppUserId_ReviewId");

            builder.HasOne(x => x.Review)
                .WithMany(x => x.Likes)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
