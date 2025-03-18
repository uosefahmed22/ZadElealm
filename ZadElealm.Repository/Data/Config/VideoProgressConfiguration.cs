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
    public class VideoProgressConfiguration : IEntityTypeConfiguration<VideoProgress>
    {
        public void Configure(EntityTypeBuilder<VideoProgress> builder)
        {
            builder.HasOne(vp => vp.User)
                .WithMany()
                .HasForeignKey(vp => vp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(vp => vp.Video)
                .WithMany()
                .HasForeignKey(vp => vp.VideoId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(vp => vp.Course)
                .WithMany()
                .HasForeignKey(vp => vp.CourseId)
                .OnDelete(DeleteBehavior.NoAction); 

            builder.HasIndex(vp => new { vp.UserId, vp.CourseId });
        }
    }
}
