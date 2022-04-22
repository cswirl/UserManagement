using api.Constants;
using api.Data.Entities;
using api.DTO;
using api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper, 
            RoleManager<IdentityRole> roleManager, ITokenService tokenService, IEmailService emailService)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
            this._roleManager = roleManager;
            this._tokenService = tokenService;
            this._emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto regDto)
        {
            if(await UserExists(regDto.Username)) return BadRequest("Username is taken");

            // Create the user 
            var user = _mapper.Map<AppUser>(regDto);
            user.UserName = regDto.Username.ToLower();

            var result = await this._userManager.CreateAsync(user, regDto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // Add User to a Member role
            var roleResult = await _userManager.AddToRoleAsync(user, ClaimNames.Role.Member.ToString());
            if (!roleResult.Succeeded) return BadRequest(result.Errors);


            // Generate Confirmation Token to be sent via Email
            var confirmationToken = await this._userManager.GenerateEmailConfirmationTokenAsync(user);

            // Build the Confirmation Link and send via Email
            /// Create the Absolute path to confirm-email action
            var request = HttpContext.Request;
            var EmailConfirmation_path = string.Concat(request.Scheme, "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        request.Path.ToUriComponent());

            EmailConfirmation_path = EmailConfirmation_path.Replace(nameof(Register).ToLower(), "confirm-email");
            ///

            var confirmationLink = QueryHelpers.AddQueryString(EmailConfirmation_path, new Dictionary<string, string>
            {
                {"userId", user.Id.ToString()},
                {"token", confirmationToken }
            });


            // Send the confirmation link via Email
            await _emailService.SendAsync(user.Email,
                "Please confirm your email",
                $"Please click on this link to confirm your email address: {confirmationLink}");


            return StatusCode(201);
        }


        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null) return Unauthorized("Invalid username");

            if (! await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new IdentityError { 
                    Code = "EmailNotConfirmed", 
                    Description = "Email is not confirmed. Please follow the confirmation link sent to your email." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut) return Unauthorized("Login failed");

                return Unauthorized("Login failed");
            }


            return Ok(new LoginResponseDto
            {
                UserName = user.UserName,
                Token = await _tokenService.CreateToken(user),
            });
        }

        [HttpGet("confirm-email")]
        public async Task<ContentResult> EmailConfirmation(string userId, string token)
        {
            var errorMessage = "<h3>Failed to validate email. Please contact administrator</h3>";
            var user = await _userManager.FindByIdAsync(userId);

            if (user == default)
            {
                base.Response.StatusCode = 400;
                return base.Content(errorMessage, "text/html");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                base.Response.StatusCode = 400;
                return base.Content(errorMessage, "text/html");
            }

            var message = "<h3>Email address is confirmed. Your account is now active</h3> <BR/><p>You can close this window and try to login.</p>";

            return base.Content(message, "text/html");
        }


        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
