using System;

namespace TimeCapital.Domain.Entities;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid? GoalId { get; set; }
    public Goal? Goal { get; set; }

    public string UserId { get; set; } = null!;

    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset? EndTimeUtc { get; set; }

    public DateTimeOffset? CanceledAtUtc { get; set; }
}
