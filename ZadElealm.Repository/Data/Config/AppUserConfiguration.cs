using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Repository.Data.Config
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.HasMany(u => u.UserNotifications).WithOne(n => n.User).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Enrollments).WithOne(e => e.AppUser).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Favorites).WithOne(f => f.AppUser).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Reviews).WithOne(r => r.User).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Ratings).WithOne(r => r.User).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(u => u.Reports).WithOne(r => r.AppUser).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
