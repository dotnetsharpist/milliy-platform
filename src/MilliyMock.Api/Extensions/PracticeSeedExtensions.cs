using System.Text.Json;
using MilliyMock.DataAccess.Contexts;
using MilliyMock.Domain.Entities;
using MilliyMock.Domain.Enums;

namespace MilliyMock.Extensions;

// Idempotent JSON seeding for the practice (mashq) question bank.
// Content lives in seed/practice-topics.json and seed/practice-questions/*.json
// so new questions are a git commit + redeploy, no migrations involved.
// Topics are deduped by (Subject, Slug); questions by exact Text match.
public static class PracticeSeedExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static void SeedPracticeData(this WebApplication app)
    {
        var seedRoot = Path.Combine(app.Environment.ContentRootPath, "seed");
        if (!Directory.Exists(seedRoot)) return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MilliyMockDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();

        try
        {
            SeedTopics(db, Path.Combine(seedRoot, "practice-topics.json"), logger);
            SeedQuestions(db, Path.Combine(seedRoot, "practice-questions"), logger);
        }
        catch (Exception ex)
        {
            // Seeding must never take the API down — log and continue.
            logger.LogError(ex, "Practice seed failed");
        }
    }

    private static void SeedTopics(MilliyMockDbContext db, string filePath, ILogger logger)
    {
        if (!File.Exists(filePath)) return;

        var topics = JsonSerializer.Deserialize<List<TopicSeed>>(File.ReadAllText(filePath), JsonOptions) ?? [];
        var inserted = 0;

        foreach (var seed in topics)
        {
            var subject = (SubjectType)seed.Subject;
            if (db.PracticeTopics.Any(t => t.Subject == subject && t.Slug == seed.Slug)) continue;

            db.PracticeTopics.Add(new PracticeTopic
            {
                Subject = subject,
                Slug = seed.Slug,
                Name = seed.Name,
                Section = seed.Section,
                Order = seed.Order,
            });
            inserted++;
        }

        if (inserted > 0)
        {
            db.SaveChanges();
            logger.LogInformation("Practice seed: {count} topics inserted", inserted);
        }
    }

    private static void SeedQuestions(MilliyMockDbContext db, string directory, ILogger logger)
    {
        if (!Directory.Exists(directory)) return;

        var inserted = 0;
        foreach (var file in Directory.EnumerateFiles(directory, "*.json").OrderBy(f => f))
        {
            var questions = JsonSerializer.Deserialize<List<QuestionSeed>>(File.ReadAllText(file), JsonOptions) ?? [];

            foreach (var seed in questions)
            {
                if (db.PracticeQuestions.Any(q => q.Text == seed.Text)) continue;

                db.PracticeQuestions.Add(new PracticeQuestion
                {
                    Subject = (SubjectType)seed.Subject,
                    Grade = seed.Grade,
                    Difficulty = (PracticeDifficulty)seed.Difficulty,
                    Topic = seed.Topic,
                    Text = seed.Text,
                    OptionA = seed.OptionA,
                    OptionB = seed.OptionB,
                    OptionC = seed.OptionC,
                    OptionD = seed.OptionD,
                    CorrectLetter = seed.CorrectLetter.Trim().ToUpperInvariant(),
                    ExplanationTitle = seed.ExplanationTitle,
                    Explanation = seed.Explanation,
                    TimeLimitSeconds = seed.TimeLimitSeconds is > 0 ? seed.TimeLimitSeconds.Value : 60,
                    IsActive = true,
                });
                inserted++;
            }
        }

        if (inserted > 0)
        {
            db.SaveChanges();
            logger.LogInformation("Practice seed: {count} questions inserted", inserted);
        }
    }

    private sealed record TopicSeed(int Subject, string Slug, string Name, string? Section, int Order);

    private sealed record QuestionSeed(
        int Subject, int? Grade, int Difficulty, string Topic, string Text,
        string OptionA, string OptionB, string OptionC, string OptionD,
        string CorrectLetter, string? ExplanationTitle, string? Explanation,
        int? TimeLimitSeconds);
}
