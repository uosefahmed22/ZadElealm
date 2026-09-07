using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
                    //if (!context.Categories.Any())
                    //{
                    //    logger.LogInformation("Seeding Categories...");
                    //    var categoriesData = File.ReadAllText(Path.Combine(basePath, "Category.json"));
                    //    var categories = JsonSerializer.Deserialize<List<Category>>(categoriesData);
                    //    await context.Categories.AddRangeAsync(categories);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Categories successfully.");
                    //}

                    //Seed Quizzes.
                    //if (!context.Quizzes.Any())
                    //{
                    //    logger.LogInformation("Seeding Quizzes...");
                    //    var quizzesData = File.ReadAllText(Path.Combine(basePath, "Quiz.json"));
                    //    var quizzes = JsonSerializer.Deserialize<List<Quiz>>(quizzesData);
                    //    await context.Quizzes.AddRangeAsync(quizzes);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Quizzes successfully.");
                    //}

                    //Seed Questions
                    //if (!context.Question.Any())
                    //{
                    //    logger.LogInformation("Seeding Questions...");
                    //    var questionsData = File.ReadAllText(Path.Combine(basePath, "Question.json"));
                    //    var questions = JsonSerializer.Deserialize<List<Question>>(questionsData);
                    //    await context.Question.AddRangeAsync(questions);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Questions successfully.");
                    //}

                    //Seed Choices
                    //if (!context.Choice.Any())
                    //{
                    //    logger.LogInformation("Seeding Choices...");
                    //    var choicesData = File.ReadAllText(Path.Combine(basePath, "Choice.json"));
                    //    var choices = JsonSerializer.Deserialize<List<Choice>>(choicesData);
                    //    await context.Choice.AddRangeAsync(choices);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Choices successfully.");
                    //}

                    //Seed Reports
                    //if (!context.Reports.Any())
                    //{
                    //    logger.LogInformation("Seeding Reports...");
                    //    var reportsData = File.ReadAllText(Path.Combine(basePath, "Report.json"));
                    //    var reports = JsonSerializer.Deserialize<List<Report>>(reportsData);
                    //    await context.Reports.AddRangeAsync(reports);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Reports successfully.");
                    //}

                    //Seed Notifications
                    //if (!context.Notifications.Any())
                    //{
                    //    logger.LogInformation("Seeding Notifications...");
                    //    var notificationsData = File.ReadAllText(Path.Combine(basePath, "Notification.json"));
                    //    var notifications = JsonSerializer.Deserialize<List<Notification>>(notificationsData);
                    //    await context.Notifications.AddRangeAsync(notifications);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded Notifications successfully.");
                    //}

                    //Seed UserNotifications
                    //if (!context.Notifications.Any())
                    //{
                    //    logger.LogInformation("Seeding UserNotifications...");
                    //    var userNotificationsData = File.ReadAllText(Path.Combine(basePath, "UserNotification.json"));
                    //    var userNotifications = JsonSerializer.Deserialize<List<UserNotification>>(userNotificationsData);
                    //    await context.UserNotifications.AddRangeAsync(userNotifications);
                    //    await context.SaveChangesAsync();
                    //    logger.LogInformation("Seeded UserNotifications successfully.");
                    //}

                    await SeedFiqhAssessmentAsync(context, logger);
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

        private static async Task SeedFiqhAssessmentAsync(AppDbContext context, ILogger logger)
        {
            var fiqhCategory = context.Categories.FirstOrDefault(category =>
                category.Name == "الفقه الإسلامي" || category.Name == "الفقه");
            if (fiqhCategory == null)
            {
                logger.LogWarning("Fiqh assessment was not seeded because the Fiqh category was not found.");
                return;
            }

            var assessment = await context.Assessments
                .Include(item => item.Forms)
                .ThenInclude(form => form.Questions)
                .ThenInclude(question => question.Choices)
                .AsSplitQuery()
                .FirstOrDefaultAsync(item => item.CategoryId == fiqhCategory.Id);
            var desiredForms = FiqhAssessmentSeedData.BuildForms();

            if (assessment == null)
            {
                assessment = new Assessment
                {
                    Name = "اختبار الفقه",
                    Description = "اختبار معرفي عام متاح بعد إكمال 80% من إحدى دورات الفقه.",
                    PassingScore = 60,
                    DurationMinutes = 30,
                    IsActive = true,
                    CategoryId = fiqhCategory.Id,
                    Forms = desiredForms
                };
                await context.Assessments.AddAsync(assessment);
            }
            else
            {
                assessment.DurationMinutes = 30;
                foreach (var desiredForm in desiredForms)
                {
                    var existingForm = assessment.Forms.FirstOrDefault(form =>
                        form.InternalCode == desiredForm.InternalCode);
                    if (existingForm == null)
                    {
                        assessment.Forms.Add(desiredForm);
                        continue;
                    }

                    existingForm.IsActive = true;
                    var needsUpgrade = existingForm.Questions.Count != 25 ||
                        existingForm.Questions.Any(question => question.DisplayOrder <= 0);
                    if (!needsUpgrade)
                        continue;

                    context.AssessmentQuestions.RemoveRange(existingForm.Questions);
                    existingForm.Questions = desiredForm.Questions;
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Fiqh assessment is ready with five internal forms and 25 questions per form.");
        }

        private static List<AssessmentForm> BuildFiqhForms()
            =>
            [
                Form("FIQH-A",
                    Question("ما المقصود بالطهارة في باب العبادات؟", "رفع الحدث وإزالة النجاسة", "تنظيف الثياب فقط", "رفع الحدث وإزالة النجاسة", "غسل اليدين فقط", "استعمال العطر"),
                    Question("كم عدد الصلوات المفروضة في اليوم والليلة؟", "خمس صلوات", "ثلاث صلوات", "أربع صلوات", "خمس صلوات", "ست صلوات"),
                    Question("متى يبدأ وقت الصيام اليومي؟", "من طلوع الفجر الصادق", "من شروق الشمس", "من طلوع الفجر الصادق", "من منتصف الليل", "من صلاة الظهر"),
                    Question("الزكاة الواجبة تُصرف لمن؟", "للمستحقين الذين حددهم الشرع", "لأي شخص دون ضابط", "للأقارب فقط", "للمستحقين الذين حددهم الشرع", "للتجارة فقط"),
                    Question("الإحرام عبادة مرتبطة أساسًا بأي نُسك؟", "الحج والعمرة", "الصلاة", "الزكاة", "الحج والعمرة", "صيام التطوع")),
                Form("FIQH-B",
                    Question("متى يُشرع التيمم بدل الوضوء؟", "عند فقد الماء أو تعذر استعماله", "مع وجود الماء والقدرة عليه دائمًا", "عند فقد الماء أو تعذر استعماله", "بعد كل صلاة", "قبل النوم فقط"),
                    Question("إلى أي جهة يتوجه المسلم في الصلاة؟", "إلى القبلة", "إلى أي جهة بلا عذر", "إلى القبلة", "إلى الشرق دائمًا", "إلى موضع السجود فقط"),
                    Question("في أي شهر يجب صيام الشهر المفروض؟", "رمضان", "محرم", "شعبان", "رمضان", "شوال"),
                    Question("أي العبادات الآتية تتعلق بالمال؟", "الزكاة", "الصلاة", "الزكاة", "الاعتكاف", "الطواف"),
                    Question("أين يكون الطواف في الحج والعمرة؟", "حول الكعبة", "بين الصفا والمروة", "في عرفات", "حول الكعبة", "في مزدلفة")),
                Form("FIQH-C",
                    Question("ما الأصل في الماء المستخدم للطهارة؟", "أن يكون طهورًا", "أن يكون ملونًا", "أن يكون ساخنًا", "أن يكون طهورًا", "أن يكون كثيرًا فقط"),
                    Question("ما الركن الذي تتضمنه الصلاة من وضع الجبهة على الأرض؟", "السجود", "القيام", "الركوع", "السجود", "التسليم"),
                    Question("بماذا ينتهي صيام اليوم في رمضان؟", "بغروب الشمس", "بصلاة العصر", "بمنتصف الليل", "بغروب الشمس", "بعد العشاء"),
                    Question("ما الشرط المرتبط بوجوب الزكاة في المال الذي له نصاب؟", "بلوغ النصاب", "كثرة الإنفاق", "بلوغ النصاب", "السفر", "المرض"),
                    Question("في أي يوم يقف الحاج بعرفة؟", "التاسع من ذي الحجة", "الأول من رمضان", "العاشر من محرم", "التاسع من ذي الحجة", "الأول من شوال")),
                Form("FIQH-D",
                    Question("أي مما يلي من أعمال الوضوء؟", "غسل الوجه", "غسل الثوب", "غسل الوجه", "قص الشعر", "لبس النعل"),
                    Question("ما أول ما يدخل به المصلي في الصلاة؟", "تكبيرة الإحرام", "السجود", "التشهد", "تكبيرة الإحرام", "التسليم"),
                    Question("من الأعذار التي تبيح الفطر في رمضان؟", "المرض الذي يشق معه الصوم", "مجرد الرغبة في الطعام", "المرض الذي يشق معه الصوم", "الانشغال بالعمل دائمًا", "السهر"),
                    Question("ما المقصود بالنصاب في باب الزكاة؟", "القدر الذي إذا بلغه المال وجبت فيه الزكاة بشروطها", "أي مبلغ يملكه الإنسان", "القدر الذي إذا بلغه المال وجبت فيه الزكاة بشروطها", "مقدار الصدقة التطوعية", "ثمن الطعام اليومي"),
                    Question("ما السعي في الحج والعمرة؟", "المشي بين الصفا والمروة", "الوقوف بعرفة", "المشي بين الصفا والمروة", "رمي الجمرات", "المبيت بمنى")),
                Form("FIQH-E",
                    Question("ما الحكم عند زوال العذر المبيح للتيمم ووجود الماء مع القدرة؟", "يعود إلى الطهارة بالماء", "يستمر في التيمم دائمًا", "يترك الصلاة", "يعود إلى الطهارة بالماء", "يؤخر كل الصلوات"),
                    Question("بماذا تُختتم الصلاة؟", "بالتسليم", "بالأذان", "بالركوع", "بالتسليم", "بتكبيرة الإحرام"),
                    Question("أي مما يلي يفسد الصيام إذا فُعل عمدًا بلا عذر؟", "الأكل والشرب", "النوم", "الاستحمام", "الأكل والشرب", "السواك"),
                    Question("ما الفرق الأساسي بين الزكاة والصدقة التطوعية؟", "الزكاة واجبة بشروط والصدقة تطوع", "كلاهما واجب دائمًا", "الصدقة لا تكون بالمال", "الزكاة واجبة بشروط والصدقة تطوع", "لا فرق بينهما"),
                    Question("ما المقصود بالوقوف بعرفة؟", "حضور الحاج بعرفة في وقته المحدد", "الطواف حول الكعبة", "حضور الحاج بعرفة في وقته المحدد", "السعي بين الصفا والمروة", "المبيت في مكة"))
            ];

        private static AssessmentForm Form(string internalCode, params AssessmentQuestion[] questions)
            => new()
            {
                InternalCode = internalCode,
                IsActive = true,
                Questions = questions.ToList()
            };

        private static AssessmentQuestion Question(
            string text,
            string correctChoice,
            params string[] choices)
            => new()
            {
                Text = text,
                Choices = choices.Select(choice => new AssessmentChoice
                {
                    Text = choice,
                    IsCorrect = choice == correctChoice
                }).ToList()
            };
    }
}
