using System;
using System.Collections.Generic;

namespace TimeCapital.Application.Sessions;

public sealed record StartSessionRequest(Guid ProjectId, Guid? GoalId);
public sealed record StartSessionResponse(Guid SessionId, DateTimeOffset StartTimeUtc);

public sealed record StopSessionResponse(
    Guid SessionId,
    DateTimeOffset StartTimeUtc,
    DateTimeOffset EndTimeUtc,
    int DurationSeconds
);

public sealed record CancelSessionResponse(
    Guid SessionId,
    DateTimeOffset StartTimeUtc,
    DateTimeOffset CanceledAtUtc
);

public sealed record ActiveSessionDto(
    Guid SessionId,
    Guid ProjectId,
    Guid? GoalId,
    DateTimeOffset StartTimeUtc
);

public sealed record ProjectListItemDto(Guid Id, string Title);
public sealed record ProjectTotalDto(Guid ProjectId, int TotalSeconds);

public sealed record DashboardStateDto(
    Guid? DefaultProjectId,
    IReadOnlyList<ProjectListItemDto> Projects,
    ActiveSessionDto? ActiveSession,
    IReadOnlyList<ProjectTotalDto> TotalsByProject
);
