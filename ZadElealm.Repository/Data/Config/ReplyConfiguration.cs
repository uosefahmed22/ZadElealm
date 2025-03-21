using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;

namespace ZadElealm.Repository.Data.Config
{
    public class ReplyConfiguration : IEntityTypeConfiguration<Reply>
    {
        public void Configure(EntityTypeBuilder<Reply> builder)
        {
            builder.Property(r => r.Text).IsRequired().HasMaxLength(500);
            builder.Property(r => r.CreatedAt).HasDefaultValueSql("GETDATE()");
            builder.HasOne(r => r.Review)
                .WithMany(r => r.Replies)
                .HasForeignKey(r => r.ReviewId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(r => r.ReplyLikes)
                .WithOne(rl => rl.Reply)
                .HasForeignKey(rl => rl.ReplyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
