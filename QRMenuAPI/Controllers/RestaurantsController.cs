
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;
using System.Security.Claims;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public RestaurantsController(ApplicationContext context, SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _signInManager = signInManager;

        }

        [HttpGet("Menu/{id}")]
        public ActionResult<Restaurant> GetMenu(int id)
        {
            if (_context.Restaurants == null)
            {
               return NotFound();
            }
            var restaurant = _context.Restaurants.Include(r => r.Categories).ThenInclude(c => c.Foods).FirstOrDefault(r => r.Id == id);

            if (restaurant == null)
            { 
               return NotFound();
            }

            return restaurant;
        }

        // GET: api/Restaurants
        [HttpGet("All")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetAllRestaurants()
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            return await _context.Restaurants.ToListAsync();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            return await _context.Restaurants.Where(s => s.StateId == 1).ToListAsync();
        }


        // GET: api/Restaurants/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            var restaurant = await _context.Restaurants.FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            return restaurant;
        }

        // PUT: api/Restaurants/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutRestaurant(int id, Restaurant restaurant)
        {

            if (User.HasClaim("RestaurantId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            _context.Entry(restaurant).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok();
        }

        // POST: api/Restaurants
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "CompanyAdministrator")]
        public int PostRestaurant(Restaurant restaurant)
        {
            ApplicationUser applicationUser = new ApplicationUser();
            RestaurantUser restaurantUser = new RestaurantUser();
            Claim claim;

            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
            applicationUser.CompanyId = restaurant.CompanyId;
            applicationUser.Email = restaurant.Email;
            applicationUser.Name = restaurant.Name;
            applicationUser.PhoneNumber = restaurant.Phone;
            applicationUser.RegisterDate = DateTime.Today;
            applicationUser.StateId = 1;
            applicationUser.UserName = restaurant.Name + restaurant.Id.ToString();
            _signInManager.UserManager.CreateAsync(applicationUser, "Admin123!").Wait();
            claim = new Claim("RestaurantId", restaurant.Id.ToString());
            _signInManager.UserManager.AddClaimAsync(applicationUser, claim).Wait();
            _signInManager.UserManager.AddToRoleAsync(applicationUser, "RestaurantAdministrator").Wait();
            restaurantUser.RestaurantId = restaurant.Id;
            restaurantUser.UserId = applicationUser.Id;
            _context.RestaurantUsers.Add(restaurantUser);
            return restaurant.Id;
        }

        // DELETE: api/Restaurants/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "CompanyAdministrator,RestaurantAdministrator")]
        public ActionResult UpdateStateRestaurant(int id, byte stateId)
        {
            if (stateId != 1 || stateId != 2 || stateId != 0)
            {
                return Content("Invalid State");
            }
            if (User.HasClaim("RestaurantId", id.ToString()) == false)
            {
                return Unauthorized();
            }
            if (_context.Restaurants == null)
            {
                return NotFound();
            }
            var restaurant = _context.Restaurants.Find(id);
            if (restaurant != null)
            {
                restaurant.StateId = stateId;
                _context.Restaurants.Update(restaurant);
                IQueryable<Category> categories = _context.Categories.Where(c => c.RestaurantId == restaurant.Id);
                foreach (Category category in categories)
                {
                    category.StateId = stateId;
                    _context.Categories.Update(category);
                    IQueryable<Food> foods = _context.Foods.Where(f => f.CategoryId == category.Id);
                    foreach (Food food in foods)
                    {
                        food.StateId = stateId;
                        _context.Foods.Update(food);
                    }
                }
                
                IQueryable<ApplicationUser> users = _context.Users.Where(u => u.CompanyId == id);
                foreach (ApplicationUser user in users)
                {
                    user.StateId = 0;
                    _context.Users.Update(user);
                    _signInManager.SignOutAsync();
                }
                
            }
           

            _context.SaveChanges();
            return NoContent();
        }




    }
}
