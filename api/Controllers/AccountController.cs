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
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private readonly IConfiguration configuration;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IMapper mapper, 
            RoleManager<IdentityRole> roleManager, ITokenService tokenService, IEmailService emailService, IConfiguration configuration)
        {
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._mapper = mapper;
            this._roleManager = roleManager;
            this._tokenService = tokenService;
            this._emailService = emailService;
            this.configuration = configuration;
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
                $"Please click on this link to confirm your email address: {confirmationLink}" +
                $"\n\nThe email confirmation link will expire in {AppGlobal.TOKEN_EXPIRY} hours");


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


        [HttpPost("forgot-password")] 
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null) return BadRequest(new IdentityError {Code= "EmailNotFound", Description = $"'{forgotPasswordDto.Email}' does not exist in our record" });

            
            // Generate Confirmation Token to be sent via Email
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callback_link = QueryHelpers.AddQueryString(forgotPasswordDto.ClientURI, new Dictionary<string, string>
            {
                {"email", user.Email},
                {"token", resetToken}
            });


            // Send the confirmation link via Email
            await _emailService.SendAsync(user.Email,
                "Password Reset link",
                $"Please click on this link to reset your password: {callback_link} " +
                $"\n\nThe password reset link will expire in {AppGlobal.TOKEN_EXPIRY} hours");

            return Ok();
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null) return BadRequest("Invalid Request");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            if (!resetPassResult.Succeeded)
            {
                var errors = resetPassResult.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok();
        }



        [HttpPost("change-email")]
        public async Task<ActionResult> EmailChange([FromBody] EmailChangeDto emailChangeDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == default) return Unauthorized();

            if (emailChangeDto.NewEmail == user.Email)
            {
                return BadRequest("This is your current email. Please use a different email");
            } else
            {
                var result = await EmailTaken(emailChangeDto.NewEmail, user.UserName);
                if (result) return BadRequest($"The email '{emailChangeDto.NewEmail}' already taken");
            }
            

            // Generate Confirmation Token to be sent via Email
            var token = await _userManager.GenerateChangeEmailTokenAsync(user, emailChangeDto.NewEmail);

            // Build the Confirmation Link and send via Email
            /// Create the Absolute path to update-email action
            var request = HttpContext.Request;
            var abs_url = string.Concat(request.Scheme, "://",
                        request.Host.ToUriComponent(),
                        request.PathBase.ToUriComponent(),
                        "/api/account/update-email");
            ///

            var callbackUrl = QueryHelpers.AddQueryString(abs_url, new Dictionary<string, string>
            {
                {"userId", user.Id.ToString()},
                {"newEmail", emailChangeDto.NewEmail},
                {"token", token}
            });


            // Send the confirmation link via Email
            await _emailService.SendAsync(emailChangeDto.NewEmail,
                "Confirm Email Change",
                $"IMPORTANT: If you did not make this request. DO NOT click the link and ignore this email." +
                $"\n\nPlease click on this link to confirm email change: {callbackUrl} " +
                $"\n\nThe link will expire in {AppGlobal.TOKEN_EXPIRY} hours");

            return Ok();
        }

        [HttpGet("update-email")]
        public async Task<ContentResult> UpdateEmail(string userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var errorMessage = "<h3>Request to change the email failed. Please contact administrator</h3>";

            if (user == default)
            {
                base.Response.StatusCode = 400;
                return base.Content(errorMessage, "text/html");
            }

            var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!result.Succeeded)
            {
                var error = result.Errors.FirstOrDefault();
                errorMessage = $"<h3>Request to change the email failed" +
                    $"\n\n<h4><strong>{error?.Code}</strong></h4> - {error?.Description}";
                base.Response.StatusCode = 400;
                return base.Content(errorMessage, "text/html");
            }

            var message = $"<h3>Your email is now changed to '{newEmail}'</h3> <BR/><p>You can close this window</p>";

            return base.Content(message, "text/html");
        }


        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        private async Task<bool> EmailTaken(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        private async Task<bool> EmailTaken(string email, string exceptUserName)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName.ToLower() != exceptUserName.ToLower() && x.Email.ToLower() == email.ToLower());
        }
    }

    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string ClientURI { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class EmailChangeDto
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }

        [Required]
        public string ClientURI { get; set; }   // Not used
    }
}
