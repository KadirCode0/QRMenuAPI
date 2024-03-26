using System;
using QRMenuAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace QRMenuAPI.Data
{
	public class ApplicationContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
		}


		public DbSet<Company>? Companies { get; set; }
		public DbSet<State>? States { get; set; }
        public DbSet<Restaurant>? Restaurants { get; set; }
        public DbSet<Category>? Categories { get; set; }
        public DbSet<Food>? Foods { get; set; }
        public DbSet<RestaurantUser>? RestaurantUsers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>().HasOne(u => u.State).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationUser>().HasOne(u => u.Company).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Restaurant>().HasOne(r => r.State).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Restaurant>().HasOne(r => r.Company).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Category>().HasOne(c => c.State).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Food>().HasOne(f => f.State).WithMany().OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<RestaurantUser>().HasOne(ru => ru.Restaurant).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RestaurantUser>().HasOne(ru => ru.ApplicationUser).WithMany().OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RestaurantUser>().HasKey(ru => new { ru.RestaurantId, ru.UserId });
            base.OnModelCreating(modelBuilder);


        }

    }
}

