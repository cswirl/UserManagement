using api.Data;
using api.Data.Entities;
using api.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Authorize]
    public class UsersController : AppBaseController
    {
        private readonly AppDbContext dbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;

        public UsersController(AppDbContext dbContext, UserManager<AppUser> userManager, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.mapper = mapper;
        }


        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers()
        {
            var users = dbContext.Users.ToList();

            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public AppUser GetUser(string username)
        {
            return dbContext.Users.FirstOrDefault(x => x.UserName == username);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == default) return NotFound();

            var profile = mapper.Map<UserProfileDto>(user);

            return Ok(profile);

        }

        [HttpPost("update")]
        public async Task<ActionResult> UpdateProfile(UserProfileDto userProfileDto)
        {
            var user = await userManager.GetUserAsync(User);

            if (user == default) return Unauthorized();

            mapper.Map(userProfileDto, user);

            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded) return BadRequest(result.Errors);


            return NoContent();

        }
    }
}