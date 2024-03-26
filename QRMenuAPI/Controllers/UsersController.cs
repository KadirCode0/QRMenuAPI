using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(ApplicationContext context, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _roleManager = roleManager;

        }

        // GET: api/Users
        [HttpGet("All")]
        [Authorize]
        public ActionResult<IEnumerable<ApplicationUser>> GetAllUsers()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            return _signInManager.UserManager.Users.ToList();
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<ApplicationUser>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return _signInManager.UserManager.Users.Where(s => s.StateId == 1).ToList();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<ApplicationUser> GetApplicationUser(string id) 
        {
         
            var applicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;

            if (applicationUser == null)
            {
                return NotFound();
            }

            return applicationUser;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public ActionResult PutApplicationUser(string id, ApplicationUser applicationUser)
        {
            var existingApplicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;

            existingApplicationUser.Email = applicationUser.Email;
            existingApplicationUser.Name = applicationUser.Name;
            existingApplicationUser.PhoneNumber = applicationUser.PhoneNumber;
            existingApplicationUser.StateId = applicationUser.StateId;

            _signInManager.UserManager.UpdateAsync(existingApplicationUser).Wait();


            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "RestaurantAdministrator")] 
        public ActionResult<ApplicationUser> PostApplicationUser(ApplicationUser applicationUser, string password, int restaurantId)
        {

            RestaurantUser restaurantUser = new RestaurantUser();
            _signInManager.UserManager.CreateAsync(applicationUser, password).Wait();
            restaurantUser.RestaurantId = restaurantId;
            restaurantUser.UserId = applicationUser.Id;
            _context.RestaurantUsers.Add(restaurantUser);
            _signInManager.UserManager.AddToRoleAsync(applicationUser, "RestaurantAdministrator").Wait();
            _context.SaveChanges();
            return Ok();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdministrator")]
        public ActionResult UpdateStateApplicationUser(string id, byte stateId)
        {

            if (stateId != 1 || stateId != 2 || stateId != 0)
            {
                return Content("Invalid State");
            }
            var applicationUser = _signInManager.UserManager.FindByIdAsync(id).Result;
            if (applicationUser == null)
            {
                return NotFound();
            }
            applicationUser.StateId = stateId;
            var currentRoles = _signInManager.UserManager.GetRolesAsync(applicationUser).Result;
            var result = _signInManager.UserManager.RemoveFromRolesAsync(applicationUser, currentRoles).Result;
            var updateResult = _signInManager.UserManager.UpdateAsync(applicationUser).Result;

            if (!result.Succeeded && !updateResult.Succeeded)
            {
                
                return BadRequest();
            }

            return NoContent();
        }



        [HttpPost("Login")]
        public bool Login(string username, string password)
        {
            Microsoft.AspNetCore.Identity.SignInResult signInResult;
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(username).Result;
            
            

            if (applicationUser == null)
            {
                return false;
            }
            signInResult = _signInManager.PasswordSignInAsync(applicationUser, password, false, false).Result;
            if( signInResult.Succeeded && applicationUser.StateId != 0 && applicationUser.StateId != 2)
            {
                return signInResult.Succeeded;
            }
            return false;
        }

        [HttpPost("Logout")]
        public bool Logout()
        {
            _signInManager.SignOutAsync();
            return true;
        }

        [HttpPost("PasswordReset")]
        public string? PasswordReset(string userName)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;
            if (applicationUser == null)
            {
                return null;
            }
          
             return _signInManager.UserManager.GeneratePasswordResetTokenAsync(applicationUser).Result;
        }

        [HttpPost("ValidateToken")]
        public ActionResult<string>? ValidateToken(string userName, string token, string newPassword)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;
            if (applicationUser == null)
            {
                return NotFound();
            }

            IdentityResult identityResult = _signInManager.UserManager.ResetPasswordAsync(applicationUser, token, newPassword).Result;
            if(identityResult.Succeeded == false)
            {
                return identityResult.Errors.First().Description;
            }
            return Ok();
        }

        /*
        [HttpPost("AssignRole")]
        [Authorize(Roles = "Administrator")]
        public void AssignRole(string userName, string roleName)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;
            IdentityRole identityRole = _roleManager.FindByNameAsync(roleName).Result;
            _signInManager.UserManager.AddToRoleAsync(applicationUser, identityRole.Name).Wait();
        }
        */

        [HttpPost("RemoveRole")]
        [Authorize(Roles = "Administrator")]
        public ActionResult RemoveRole(string userName)
        {
            ApplicationUser applicationUser = _signInManager.UserManager.FindByNameAsync(userName).Result;

            if (applicationUser == null)
            {
                return NotFound(); 
            }

            var currentRoles = _signInManager.UserManager.GetRolesAsync(applicationUser).Result;

            var result = _signInManager.UserManager.RemoveFromRolesAsync(applicationUser, currentRoles).Result;

            if (result.Succeeded)
            {
                return Ok(); 
            }
            else
            {
                return BadRequest(); 
            }
        }
    }
}
