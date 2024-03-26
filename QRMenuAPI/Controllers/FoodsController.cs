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
    public class FoodsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FoodsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Foods
        [HttpGet("All")]
        [Authorize]
        public ActionResult<IEnumerable<Food>> GetAllFoods()
        {
            if (_context.Foods == null)
            {
                return NotFound();
            }
            return _context.Foods.ToList();
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<Food>> GetFoods()
        {
            if (_context.Foods == null)
            {
                return NotFound();
            }
            return _context.Foods.Where(s => s.StateId == 1).ToList();
        }

        // GET: api/Foods/5
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult<Food> GetFood(int id)
        {
            if (_context.Foods == null)
            {
                return NotFound();
            }
            var food = _context.Foods.Find(id);

            if (food == null)
            {
                return NotFound();
            }

            return food;
        }

        // PUT: api/Foods/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult PutFood(int id, Food food)
        {

            var category = _context.Categories.Find(food.CategoryId);

            if (User.HasClaim("RestauranId", category.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }

            if (id != food.Id)
            {
                return BadRequest();
            }

            _context.Entry(food).State = EntityState.Modified;
            _context.SaveChanges();
            return Ok();
            
        }

        [HttpPost("UploadImage")]
        public ActionResult<string> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                string fileExtension = Path.GetExtension(file.FileName);
                string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Image", uniqueFileName);
                if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "Image")))
                {
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "Image"));
                }

                // Resmi kaydedin
                using (FileStream fileStream = new(imagePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                // Resmin yolunu döndürün
                return Ok(imagePath);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex}");
            }
        }

        // POST: api/Foods
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult<Food> PostFood(Food food)
        {
            
            food.ImgPath = food.ImgPath;
            _context.Foods.Add(food);
            _context.SaveChanges();

            return Ok();
        }

        // DELETE: api/Foods/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "RestaurantAdministrator")]
        public ActionResult UpdateStateFood(int id, byte stateId)
        {

            if(stateId != 1 || stateId != 2 || stateId != 0)
            {
                return Content("Invalid State");
            }
            var food = _context.Foods.Find(id);
            var category = _context.Categories.Find(food.CategoryId);
            if (User.HasClaim("RestauranId", category.RestaurantId.ToString()) == false)
            {
                return Unauthorized();
            }

            if (_context.Foods == null)
            {
                return NotFound();
            }
            
            if (food == null)
            {
                return NotFound();
            }

            food.StateId = stateId;
            _context.Foods.Update(food);
            _context.SaveChanges();

            return NoContent();
        }

    }
}
