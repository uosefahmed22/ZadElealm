using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZadElealm.Core.Models;
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
                    if(!context.Quizzes.Any()) {
                        logger.LogInformation("Seeding Quizzes...");
                        var quizzesData = File.ReadAllText(Path.Combine(basePath, "Quiz.json"));
                        var quizzes = JsonSerializer.Deserialize<List<Quiz>>(quizzesData);
                        await context.Quizzes.AddRangeAsync(quizzes);
                        await context.SaveChangesAsync();
                        logger.LogInformation("Seeded Quizzes successfully.");
                    }
                    
                    //Seed Questions
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


                    ///Seed Courses
                    ///if (!context.Courses.Any())
                    ///{
                    ///    logger.LogInformation("Seeding Courses...");
                    ///    var coursesData = File.ReadAllText(Path.Combine(basePath, "Courses.json"));
                    ///    var courses = JsonSerializer.Deserialize<List<Course>>(coursesData);
                    ///    await context.Courses.AddRangeAsync(courses);
                    ///    await context.SaveChangesAsync();
                    ///    logger.LogInformation("Seeded Courses successfully.");
                    ///}

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
