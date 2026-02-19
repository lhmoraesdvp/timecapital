namespace TimeCapital.Web.Security;

public sealed class MockCurrentUser : ICurrentUser
{
    public string UserId => "luis";
    public string DisplayName => "Luis";
}
