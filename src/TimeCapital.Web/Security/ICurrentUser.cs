namespace TimeCapital.Web.Security;

public interface ICurrentUser
{
    string UserId { get; }
    string DisplayName { get; }
}
