using Microsoft.AspNetCore.Identity;
using System;

namespace TimeCapital.Data.Identity;

public class ApplicationUser : IdentityUser
{
    public Guid? DefaultProjectId { get; set; }
}