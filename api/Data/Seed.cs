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
        public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Returns true If there is any existing user, return
            if (await userManager.Users.AnyAsync() & await roleManager.Roles.AnyAsync()) return;

            //// Create Users
            //var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            //var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            //if (users == null) return;

            var s_admin = "Admin";
            var s_mod = "Moderator";
            var s_member = "Member";

            // Create Roles
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
            
            
            // Create Admin
            var admin = new AppUser
            {
                UserName = "admin",
                Email = "admin@timehorizon.com",
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(admin, "admin");
            await userManager.AddToRoleAsync(admin, s_admin);


            // Create Claims for the Roles
            // Admin, Moderator
            var tier = new List<Claim>
            {
                new Claim("MembershipTier", "Silver"),
                new Claim("MembershipTier", "Gold"),
                new Claim("MembershipTier", "Platinum")
            };

            foreach (var claim in tier)
            {
                await roleManager.AddClaimAsync(roleManager.Roles.FirstOrDefault(x => x.Name == s_admin), claim);
                await roleManager.AddClaimAsync(roleManager.Roles.FirstOrDefault(x => x.Name == s_mod), claim);
            }

            // Member
            await roleManager.AddClaimAsync(
                roleManager.Roles.FirstOrDefault(x => x.Name == s_member),
                new Claim("MembershipTier", "Silver"));

        }
    }
}
