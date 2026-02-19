using System;
using System.Collections.Generic;

namespace TimeCapital.Domain.Entities;

public enum ProjectStatus
{
    Active = 1,
    Archived = 2
}

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = null!;
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public string OwnerId { get; set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
