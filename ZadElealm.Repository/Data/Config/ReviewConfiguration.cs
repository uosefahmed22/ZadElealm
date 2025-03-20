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
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("Reviews");

            builder.Property(x => x.Text)
                .IsRequired();

            builder.HasOne(x => x.Course)
                .WithMany(c => c.Review) 
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.User)
              .WithMany(u => u.Reviews)
              .HasForeignKey(r => r.AppUserId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore("AppUserId1");
            builder.Ignore("CourseId1");
        }
    }
}
