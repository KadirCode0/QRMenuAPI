using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Data;
using QRMenuAPI.Models;

namespace QRMenuAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet("All")]
        [Authorize]
        public ActionResult<IEnumerable<Category>> GetAllCategories()
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            return _context.Categories.ToList();
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }
            return _context.Categories.Where(s => s.StateId == 1).ToList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Category> GetCategory(int id)
        {
            if (_context.Categories == null)
            {
                return NotFound();
            }

            var category = _context.Categories.Include(f => f.Foods).FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutCategory(int id, Category category)
        {

            var restaurantId = _context.Categories.Find(id);
            if (User.HasClaim("RestaurantId", restaurantId.ToString()) == false)
            {
                return Unauthorized();
            }

            if (id != category.Id)
            {
                return BadRequest();
            }

            _context.Entry(category).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok();


        }

        // POST: api/Categories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "RestaurantAdministrator")]
        public string PostCategory(Category category)
        {
     
            _context.Categories.Add(category);
            _context.SaveChanges();

            return category.Name;
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult UpdateStateCategory(int id, byte stateId)
        {
            if (stateId != 1 || stateId != 2 || stateId != 0)
            {
                return Content("Invalid State");
            }
            var restaurantId = _context.Categories.Find(id);
            if (User.HasClaim("RestaurantId", restaurantId.ToString()) == false)
            {
                return Unauthorized();
            }


            if (_context.Categories == null)
            {
                return NotFound();
            }
            var category =  _context.Categories.Find(id);
            if (category != null)
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

            
            _context.SaveChanges();

            return NoContent();
        }

       
    }
}
