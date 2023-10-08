using Microsoft.AspNetCore.Identity;

namespace IdentityApp.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; }
    public string Address { get; set; }

    public AppUser(string fullName, string address)
    {
        FullName = fullName;
        Address = address;
    }
}
