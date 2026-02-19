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

public sealed record LastSessionDto(
    Guid SessionId,
    Guid ProjectId,
    string ProjectTitle,
    Guid? GoalId,
    DateTimeOffset StartTimeUtc,
    DateTimeOffset EndTimeUtc,
    int DurationSeconds
);

public sealed record DashboardStateDto(
    Guid? DefaultProjectId,
    string? DefaultProjectTitle,
    IReadOnlyList<ProjectListItemDto> Projects,
    ActiveSessionDto? ActiveSession,

    // Totais (UTC, MVP simples)
    int DefaultProjectTotalSeconds,
    int TodayTotalSeconds,
    int WeekTotalSeconds,

    // Meta (quando aplicável: se sessão ativa tem GoalId)
    int? ActiveGoalTargetSeconds,

    // Últimas sessões do projeto default
    IReadOnlyList<LastSessionDto> LastSessions,

    // (mantém pra futuro)
    IReadOnlyList<ProjectTotalDto> TotalsByProject
);
