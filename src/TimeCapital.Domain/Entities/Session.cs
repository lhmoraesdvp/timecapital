namespace TimeCapital.Domain.Entities;

public class Session
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public int DurationMinutes { get; set; }

    public int AreaId { get; set; }
    public Area? Area { get; set; }
}
