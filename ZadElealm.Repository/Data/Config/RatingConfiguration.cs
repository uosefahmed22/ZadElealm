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
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.Property(x => x.Value).HasColumnType("decimal(18,2)");

            builder.HasIndex(rating => new { rating.AppUserId, rating.courseId })
                .IsUnique()
                .HasDatabaseName("UX_Ratings_AppUserId_courseId")
                .HasFilter("[IsDeleted] = 0");

        }
    }
}
