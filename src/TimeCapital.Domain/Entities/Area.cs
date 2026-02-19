namespace TimeCapital.Domain.Entities;

public sealed class Area
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string NormalizedName { get; set; } = default!;
    public string? Color { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public static string NormalizeName(string name)
        => string.Join(' ', (name ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();
}
