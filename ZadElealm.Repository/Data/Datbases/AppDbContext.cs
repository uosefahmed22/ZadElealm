﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;

namespace ZadElealm.Repository.Data.Datbases
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Course>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Enrollment>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Favorite>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Rating>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Review>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Report>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Progress>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<UserNotification>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Certificate>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Quiz>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<VideoProgress>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Video>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Question>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Choice>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Reply> replies { get; set; }
        public DbSet<ReviewLike> ReviewLikes { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<VideoProgress> VideoProgresses { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<Choice> Choice { get; set; }
        public DbSet<UserRank> userRanks { get; set; }
    }
}
