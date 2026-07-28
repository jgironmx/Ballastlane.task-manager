using Ballastlane.Tasks.Domain.Exceptions;

namespace Ballastlane.Tasks.Domain.Tasks;

/// <summary>
/// A personal task owned by exactly one user. Instances are only ever created or mutated
/// through the methods below, so every invariant is enforced in one place.
/// </summary>
public sealed class TaskItem
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public Guid Id { get; private set; }

    public Guid OwnerId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public TaskItemStatus Status { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    private TaskItem()
    {
    }

    /// <summary>
    /// Creates a new task owned by <paramref name="ownerId"/>. <paramref name="nowUtc"/> and
    /// <paramref name="currentBusinessDate"/> are supplied by the caller (via an abstract clock)
    /// rather than read from the system clock here, so creation stays deterministic and testable.
    /// </summary>
    public static TaskItem Create(
        Guid ownerId,
        string title,
        string? description,
        DateOnly? dueDate,
        DateTimeOffset nowUtc,
        DateOnly currentBusinessDate)
    {
        EnsureValidOwnerId(ownerId);
        var normalizedTitle = EnsureValidTitle(title);
        var normalizedDescription = EnsureValidDescription(description);
        EnsureDueDateNotInThePast(dueDate, currentBusinessDate);

        return new TaskItem
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = normalizedTitle,
            Description = normalizedDescription,
            Status = TaskItemStatus.Pending,
            DueDate = dueDate,
            CreatedAtUtc = nowUtc,
            UpdatedAtUtc = null,
        };
    }

    /// <summary>
    /// Updates the editable details of the task. Ownership and status are intentionally not
    /// changeable here — see <see cref="ChangeStatus"/> and the immutable <see cref="OwnerId"/>.
    /// The due-date-not-in-the-past rule is enforced only at creation (see invariant list in
    /// docs/decisions/ADR-006-taskitem-domain-model.md); an existing task's due date may legally
    /// become "in the past" simply by the passage of time, and updating other fields should not
    /// be blocked by that.
    /// </summary>
    public void UpdateDetails(string title, string? description, DateOnly? dueDate, DateTimeOffset nowUtc)
    {
        var normalizedTitle = EnsureValidTitle(title);
        var normalizedDescription = EnsureValidDescription(description);

        Title = normalizedTitle;
        Description = normalizedDescription;
        DueDate = dueDate;
        UpdatedAtUtc = nowUtc;
    }

    public void ChangeStatus(TaskItemStatus status, DateTimeOffset nowUtc)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException($"'{status}' is not a valid task status.");
        }

        Status = status;
        UpdatedAtUtc = nowUtc;
    }

    private static void EnsureValidOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
        {
            throw new DomainException("Owner id cannot be empty.");
        }
    }

    private static string EnsureValidTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title is required.");
        }

        var trimmed = title.Trim();
        if (trimmed.Length > TitleMaxLength)
        {
            throw new DomainException($"Title cannot exceed {TitleMaxLength} characters.");
        }

        return trimmed;
    }

    private static string? EnsureValidDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var trimmed = description.Trim();
        if (trimmed.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Description cannot exceed {DescriptionMaxLength} characters.");
        }

        return trimmed;
    }

    private static void EnsureDueDateNotInThePast(DateOnly? dueDate, DateOnly currentBusinessDate)
    {
        if (dueDate.HasValue && dueDate.Value < currentBusinessDate)
        {
            throw new DomainException("Due date cannot be earlier than the current date.");
        }
    }
}
