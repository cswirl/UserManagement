using api.Constants;
using api.Data.Entities;
using api.DTO;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Controllers
{
    
    public class AccountController : AppBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper, 
            RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
            this._roleManager = roleManager;
            this._tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto regDto)
        {
            if(await UserExists(regDto.Username)) return BadRequest("Username is taken");

            // Validating Email address (Optional) - check if it doesn't exist in the database
            // THIS VALIDATION IS SET IN THE STARTUP CONFIGURATION: options.User.RequireUniqueEmail = true;

            // Create the user 
            var user = _mapper.Map<AppUser>(regDto);
            user.UserName = regDto.Username.ToLower();

            var result = await this._userManager.CreateAsync(user, regDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, ClaimNames.Role.Member.ToString());

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            // Generate Confirmation and send via Email


            return Ok();
        }


        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(
                loginDto.Username,
                loginDto.Password,
                loginDto.RememberMe,
                false);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut) return Unauthorized("Login failed");

                return Unauthorized("Login failed");
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            return Ok(new LoginResponseDto
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
