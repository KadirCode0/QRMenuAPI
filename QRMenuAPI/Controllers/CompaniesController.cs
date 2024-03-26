using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;


namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public CompaniesController(ApplicationContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;

        }

        // GET: api/Companies
        [Authorize]
        [HttpGet("All")]
        public ActionResult<IEnumerable<Company>> GetAllCompanies()
        {
          if (_context.Companies == null)
          {
              return NotFound();
          }
            return _context.Companies.ToList();
        }

        [Authorize]
        [HttpGet]
        public ActionResult<IEnumerable<Company>> GetCompanies()
        {
            if (_context.Companies == null)
            {
                return NotFound();
            }
            return _context.Companies.Where(s => s.StateId == 1).ToList();
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Company> GetCompany(int id)
        {

            if (_context.Companies == null)
            {
                  return NotFound();
            }
            var company = _context.Companies.Find(id);

            if (company == null)
            {
                return NotFound();
            }

            return company;
        }

        // PUT: api/Companies/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "CompanyAdministrator")]
        public ActionResult PutCompany(int id, Company company)
        {
            if(User.HasClaim("CompanyId", id.ToString()) == false)
            {
                return Unauthorized();
            }

            if (id != company.Id)
            {
                return BadRequest();
            }

            _context.Entry(company).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok();
          
        }

        // POST: api/Companies
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public int PostCompany(Company company)
        {
            Claim claim;

            ApplicationUser applicationUser = new ApplicationUser();
            _context.Companies.Add(company);
            _context.SaveChanges();
            applicationUser.CompanyId = company.Id;
            applicationUser.Email = company.Email;
            applicationUser.Name = company.Name;
            applicationUser.PhoneNumber = company.Phone;
            applicationUser.RegisterDate = DateTime.Today;
            applicationUser.StateId = 1;
            applicationUser.UserName = company.Name + company.Id.ToString();
            _signInManager.UserManager.CreateAsync(applicationUser, "Admin123!").Wait();
            claim = new Claim("CompanyId", company.Id.ToString());
            _signInManager.UserManager.AddClaimAsync(applicationUser, claim).Wait();
            _signInManager.UserManager.AddToRoleAsync(applicationUser, "CompanyAdministrator").Wait();

            return company.Id;
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator, CompanyAdministrator")]
        public ActionResult UpdateStateCompany(int id, byte stateId) 
        {

            if (stateId != 1 || stateId != 2 || stateId != 0)
            {
                return Content("Invalid State");
            }

            if (User.HasClaim("CompanyId", id.ToString()) == false || User.IsInRole("Administrator") == false)
            {
                return Unauthorized();
            }

            if (_context.Companies == null)
            {
                return NotFound();
            }
            var company = _context.Companies.Find(id);
            if (company != null)
            {
                company.StateId = stateId;
                _context.Companies.Update(company);
                IQueryable<Restaurant> restaurants = _context.Restaurants.Where(r => r.CompanyId == id);
                //RestaurantsController restaurantsController = new RestaurantsController(_context);
                foreach (Restaurant restaurant in restaurants)
                {
                    //restaurantsController.DeleteRestaurant(restaurant.Id);
                    restaurant.StateId = stateId;
                    _context.Restaurants.Update(restaurant);
                    IQueryable<Category> categories = _context.Categories.Where(c => c.RestaurantId == restaurant.Id);
                    foreach (Category category in categories)
                    {
                        category.StateId = 0;
                        _context.Categories.Update(category);
                        IQueryable<Food> foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                        foreach (Food food in foods)
                        {
                            food.StateId = stateId;
                            _context.Foods.Update(food);
                        }
                    }
                }
                IQueryable<ApplicationUser> users = _context.Users.Where(u => u.CompanyId == id);
                foreach (ApplicationUser user in users)
                {
                    user.StateId = stateId;
                    _context.Users.Update(user);
                    _signInManager.SignOutAsync();
                }
            }
            _context.SaveChanges();
            return NoContent();
        }

        
    }
}
