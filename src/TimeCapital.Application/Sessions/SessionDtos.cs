// TimeCapital.Application/Sessions/SessionDtos.cs
namespace TimeCapital.Application.Sessions;

// ===== Requests / Responses =====
public sealed record StartSessionRequest(Guid ProjectId, Guid? GoalId);

public sealed record StartSessionResponse(Guid SessionId, DateTimeOffset StartTimeUtc);

public sealed record StopSessionResponse(
    Guid SessionId,
    DateTimeOffset StartTimeUtc,
    DateTimeOffset EndTimeUtc,
    int DurationSeconds);

// ===== Dashboard DTOs =====
public sealed record ProjectListItemDto(Guid Id, string Title);

public sealed record ActiveSessionDto(
    Guid SessionId,
    Guid ProjectId,
    Guid? GoalId,
    DateTimeOffset StartTimeUtc);

public sealed record LastSessionDto(
    Guid SessionId,
    Guid ProjectId,
    string ProjectTitle,
    Guid? GoalId,
    DateTimeOffset StartTimeUtc,
    DateTimeOffset EndTimeUtc,
    int DurationSeconds);

public sealed record ProjectTotalDto(
    Guid ProjectId,
    string ProjectTitle,
    int TotalSeconds);

// NOVO: Totais por dia (últimos 7 dias)
public sealed record DayTotalDto(
    DateOnly Day,          // ex: 2026-02-19
    int TotalSeconds);     // soma das sessões desse dia (já filtrado no backend)

// ATUALIZADO: inclui TotalsByProject + Last7Days + DebugVersion
public sealed record DashboardStateDto(
    Guid? DefaultProjectId,
    string? DefaultProjectTitle,
    IReadOnlyList<ProjectListItemDto> Projects,
    ActiveSessionDto? ActiveSession,

    int DefaultProjectTotalSeconds,
    int TodayTotalSeconds,
    int WeekTotalSeconds,

    int? ActiveGoalTargetSeconds,

    IReadOnlyList<LastSessionDto> LastSessions,

    // distribuição da semana por projeto (já existe no seu JSON)
    IReadOnlyList<ProjectTotalDto> TotalsByProject,

    // NOVO: barras "Horas por dia" (últimos 7 dias, filtrado pelo projectId escolhido no front)
    IReadOnlyList<DayTotalDto> Last7Days,

    // opcional pra debug/versão
    string? DebugVersion = null
);
