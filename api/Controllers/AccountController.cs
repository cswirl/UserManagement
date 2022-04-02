using api.Data.Entities;
using api.DTO;
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

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
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

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            // Generate Confirmation and send via Email


            return Ok();
        }


        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto loginDto)
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

            return Ok();
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
