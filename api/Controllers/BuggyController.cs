using api.Data;
using api.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    public class BuggyController : AppBaseController
    {
        private readonly AppDbContext _context;

        public BuggyController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("test-auth")]
        public ActionResult TestUnauthorized()
        {
            return Ok(new { Text = "Authorization passed" });

            //// Will cause an subtle error in Angular side - during parsing
            //return Ok("Authorization passed");
        }


        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            return NotFound();
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var thing = _context.Users.Find(-1);

            var thingToReturn = thing.ToString();

            return thingToReturn;
        }


        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest();
        }

    }
}
