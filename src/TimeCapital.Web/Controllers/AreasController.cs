private readonly ApplicationDbContext _db;
private readonly ICurrentUser _currentUser;

public AreasController(ApplicationDbContext db, ICurrentUser currentUser)
{
    _db = db;
    _currentUser = currentUser;
}