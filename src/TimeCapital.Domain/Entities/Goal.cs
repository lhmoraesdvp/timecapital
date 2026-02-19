using System;

namespace TimeCapital.Domain.Entities;

public class Goal
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int TargetMinutes { get; set; }
}
