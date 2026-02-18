namespace TimeCapital.Domain.Entities;

public class Goal
{
    public int Id { get; set; }
    public int TargetMinutes { get; set; }

    public int AreaId { get; set; }
    public Area? Area { get; set; }
}
