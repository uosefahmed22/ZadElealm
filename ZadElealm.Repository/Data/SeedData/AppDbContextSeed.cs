using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
using ZadElealm.Core.Models.Identity;
using ZadElealm.Repository.Data.Datbases;

namespace ZadElealm.Repository.Data.SeedData
{
    public static class AppDbContextSeed
    {
        public static async Task SeedAsync(AppDbContext context, ILogger logger)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    var basePath = "../ZadElealm.Repository/Data/SeedData/Data";
                    //Seed Categories
                    if (!context.Categories.Any())
                    {
                        logger.LogInformation("Seeding Categories...");
                        var categoriesData = File.ReadAllText(Path.Combine(basePath, "Category.json"));
                        var categories = JsonSerializer.Deserialize<List<Category>>(categoriesData);
                        await context.Categories.AddRangeAsync(categories);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Categories successfully.");
                    }

                    //Seed Quizzes.
                    if (!context.Quizzes.Any())
                    {
                        logger.LogInformation("Seeding Quizzes...");
                        var quizzesData = File.ReadAllText(Path.Combine(basePath, "Quiz.json"));
                        var quizzes = JsonSerializer.Deserialize<List<Quiz>>(quizzesData);
                        await context.Quizzes.AddRangeAsync(quizzes);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Quizzes successfully.");
                    }

                    ////Seed Questions
                    if (!context.Question.Any())
                    {
                        logger.LogInformation("Seeding Questions...");
                        var questionsData = File.ReadAllText(Path.Combine(basePath, "Question.json"));
                        var questions = JsonSerializer.Deserialize<List<Question>>(questionsData);
                        await context.Question.AddRangeAsync(questions);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Questions successfully.");
                    }

                    //Seed Choices
                    if (!context.Choice.Any())
                    {
                        logger.LogInformation("Seeding Choices...");
                        var choicesData = File.ReadAllText(Path.Combine(basePath, "Choice.json"));
                        var choices = JsonSerializer.Deserialize<List<Choice>>(choicesData);
                        await context.Choice.AddRangeAsync(choices);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Choices successfully.");
                    }

                    //Seed Reports
                    if (!context.Reports.Any())
                    {
                        logger.LogInformation("Seeding Reports...");
                        var reportsData = File.ReadAllText(Path.Combine(basePath, "Report.json"));
                        var reports = JsonSerializer.Deserialize<List<Report>>(reportsData);
                        await context.Reports.AddRangeAsync(reports);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Reports successfully.");
                    }

                    //Seed Notifications
                    if (!context.Notifications.Any())
                    {
                        logger.LogInformation("Seeding Notifications...");
                        var notificationsData = File.ReadAllText(Path.Combine(basePath, "Notification.json"));
                        var notifications = JsonSerializer.Deserialize<List<Notification>>(notificationsData);
                        await context.Notifications.AddRangeAsync(notifications);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Notifications successfully.");
                    }

                    //Seed UserNotifications
                    if (!context.Notifications.Any())
                    {
                        logger.LogInformation("Seeding UserNotifications...");
                        var userNotificationsData = File.ReadAllText(Path.Combine(basePath, "UserNotification.json"));
                        var userNotifications = JsonSerializer.Deserialize<List<UserNotification>>(userNotificationsData);
                        await context.UserNotifications.AddRangeAsync(userNotifications);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded UserNotifications successfully.");
                    }

                    await transaction.CommitAsync();
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the database.");
                    await transaction.RollbackAsync();
                }
            }
        }
    }
}
