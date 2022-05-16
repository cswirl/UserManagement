using api.Constants;
using api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace api.Data
{
    public static class Seed
    {
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext appDbContext)
        {
            var s_admin = ClaimNames.Role.Admin.ToString();
            var s_mod = ClaimNames.Role.Moderator.ToString();
            var s_member = ClaimNames.Role.Member.ToString();

            // Create Roles if no record
            if (!(await roleManager.Roles.AnyAsync()))
            {
                var roles = new List<IdentityRole>
                {
                    new IdentityRole{Name = s_admin},
                    new IdentityRole{Name = s_mod},
                    new IdentityRole{Name = s_member}
                };

                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }

                // If an admin user is found, re-assign admin to admin role
                var admin = await userManager.FindByNameAsync("admin");
                if (admin != null)
                {
                    await userManager.AddToRoleAsync(admin, s_admin);
                }
            }

            // Create Claims for the Roles if no record
            if (!(await appDbContext.RoleClaims.AnyAsync()))
            {
                // Admin, Moderator
                var tier = new List<Claim>
                {
                    new Claim(ClaimNames.MembershipTier, ClaimNames.MembershipTierEnum.Silver.ToString()),
                    new Claim(ClaimNames.MembershipTier, ClaimNames.MembershipTierEnum.Gold.ToString()),
                    new Claim(ClaimNames.MembershipTier, ClaimNames.MembershipTierEnum.Platinum.ToString())
                };

                var admin = roleManager.Roles.FirstOrDefault(x => x.Name == s_admin);
                var mod = roleManager.Roles.FirstOrDefault(x => x.Name == s_mod);

                foreach (var claim in tier)
                {
                    if (admin != default) await roleManager.AddClaimAsync(admin, claim);
                    if (mod != default) await roleManager.AddClaimAsync(mod, claim);
                }

                // Member
                await roleManager.AddClaimAsync(
                    roleManager.Roles.FirstOrDefault(x => x.Name == s_member),
                    new Claim(ClaimNames.MembershipTier, ClaimNames.MembershipTierEnum.Silver.ToString()));
            }


            // Create Admin if no record
            if (!(await userManager.Users.AnyAsync()))
            {
                var admin = new AppUser
                {
                    UserName = "admin",
                    Email = "admin@timehorizon.com",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(admin, "Pa$$w0rd");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, s_admin);
                }
            }

        }
    }
}
