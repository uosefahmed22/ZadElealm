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
