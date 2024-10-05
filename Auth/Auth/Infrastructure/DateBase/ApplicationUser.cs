using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.DateBase
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        /// <summary>
        /// FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// LastName
        /// </summary>
        public string LastName { get; set; }
    }
}
