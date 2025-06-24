using Microsoft.AspNetCore.Identity;

namespace Engli3m.Domain.Enities
{
    public class Role : IdentityRole<int>
    {
        public Role() { }

        public Role(string roleName) : base(roleName)
        {
            // Ensures the Name property is set
            Name = roleName;
        }

        public string Description { get; set; } = string.Empty;
    }
}