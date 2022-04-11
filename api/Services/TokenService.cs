using api.Data;
using api.Data.Entities;
using api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _appDbContext;

        public TokenService(IConfiguration config, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext appDbContext)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._appDbContext = appDbContext;
        }

        public async Task<string> CreateToken(AppUser user)
        {
            var claims = await CollectClaims(user);

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private async Task<IEnumerable<Claim>> CollectClaims(AppUser user)
        {
            // Create default claim
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            // Get Roles and add to claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            // Get Roles claims and add to claims i.e. MembershipTiers
            foreach(var role in roles)
            {
                var x = _appDbContext.Roles.Where(r => r.Name == role).FirstOrDefault();
                claims.AddRange(await _roleManager.GetClaimsAsync(x));
            }

            return claims;
        }
    }
}
