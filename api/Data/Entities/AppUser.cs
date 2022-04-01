using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Data.Entities
{
    public class AppUser : IdentityUser
    {
        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
