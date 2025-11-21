using System.Text;

namespace BlazorTodoApp.Models;

public sealed class TodoItem
{
    public Guid Id { get; init; }
    public string Title { get; private set; }
    public string? Note { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateOnly? DueDay { get; private set; }

    private TodoItem(
        Guid id,
        string title,
        string? note,
        bool isCompleted,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateOnly? dueDay)
    {
        Id = id;
        Title = title;
        Note = note;
        IsCompleted = isCompleted;
        CreatedAt = EnsureUtc(createdAt);
        UpdatedAt = EnsureUtc(updatedAt);
        DueDay = dueDay;
    }

    public static TodoItem Create(string title, string? note, DateOnly? dueDay, DateTimeOffset timestampUtc)
    {
        var cleanedTitle = ValidateTitle(title);
        var cleanedNote = CleanNote(note);
        var validatedDueDay = ValidateDueDay(dueDay, timestampUtc);
        var utcTimestamp = EnsureUtc(timestampUtc);

        return new TodoItem(
            Guid.NewGuid(),
            cleanedTitle,
            cleanedNote,
            isCompleted: false,
            createdAt: utcTimestamp,
            updatedAt: utcTimestamp,
            dueDay: validatedDueDay);
    }

    public static TodoItem Rehydrate(
        Guid id,
        string title,
        string? note,
        bool isCompleted,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateOnly? dueDay)
    {
        return new TodoItem(
            id == Guid.Empty ? Guid.NewGuid() : id,
            ValidateTitle(title),
            CleanNote(note),
            isCompleted,
            createdAt,
            updatedAt,
            dueDay);
    }

    public void UpdateDetails(string title, string? note, DateOnly? dueDay, DateTimeOffset timestampUtc)
    {
        Title = ValidateTitle(title);
        Note = CleanNote(note);
        DueDay = ValidateDueDay(dueDay, timestampUtc);
        UpdatedAt = EnsureUtc(timestampUtc);
    }

    public void SetCompletion(bool isCompleted, DateTimeOffset timestampUtc)
    {
        IsCompleted = isCompleted;
        UpdatedAt = EnsureUtc(timestampUtc);
    }

    public string GetNormalizedTitleKey() => BuildNormalizedTitleKey(Title);

    public static string BuildNormalizedTitleKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        var builder = new StringBuilder(trimmed.Length);

        foreach (var ch in trimmed)
        {
            if (char.IsWhiteSpace(ch))
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }

    private static string ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        var trimmed = title.Trim();

        if (trimmed.Length is < 1 or > 120)
        {
            throw new ArgumentException("Title must be between 1 and 120 characters.", nameof(title));
        }

        return trimmed;
    }

    private static string? CleanNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        var trimmed = note.Trim();

        if (trimmed.Length > 500)
        {
            throw new ArgumentException("Note cannot exceed 500 characters.", nameof(note));
        }

        return trimmed;
    }

    private static DateOnly? ValidateDueDay(DateOnly? dueDay, DateTimeOffset timestampUtc)
    {
        if (dueDay is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(EnsureUtc(timestampUtc).UtcDateTime.Date);

        if (dueDay < today)
        {
            throw new ArgumentException("Due day cannot be set in the past.", nameof(dueDay));
        }

        return dueDay;
    }

    private static DateTimeOffset EnsureUtc(DateTimeOffset value) =>
        value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();
}
