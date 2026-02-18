namespace TimeCapital.Domain.Entities;

public class Area
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
