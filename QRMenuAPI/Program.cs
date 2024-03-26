using QRMenuAPI.Data;
using Microsoft.EntityFrameworkCore;
using QRMenuAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace QRMenuAPI;

public class Program
{
    public static void Main(string[] args)
    {
        State state;
        IdentityRole identityRole;
        ApplicationUser applicationUser;
        Company? company = null;

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<ApplicationContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDatabase")));
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllers();
        {
            ApplicationContext? context = app.Services.CreateScope().ServiceProvider.GetService<ApplicationContext>();
            if (context != null)
            {
                context.Database.Migrate();
                if (context.States.Count() == 0)
                {
                    state = new State();
                    state.Id = 0;
                    state.Name = "Deleted";
                    context.States.Add(state);
                    state = new State();
                    state.Id = 1;
                    state.Name = "Active";
                    context.States.Add(state);
                    state = new State();
                    state.Id = 2;
                    state.Name = "Passive";
                    context.States.Add(state);
                }
                if (context.Companies!.Count() == 0)
                {
                    company = new Company();
                    company.Address = "Dikilitaş, Emirhan Cd. 145/A,  Beşiktaş/İstanbul";
                    company.Email = "postmaster@tabgida.com.tr";
                    company.Name = "TAB GIDA";
                    company.Phone = "(0212) 310 66 00";
                    company.PostalCode = "34349";
                    company.RegisterDate = DateTime.Today;
                    company.StateId = 1;
                    company.TaxNumber = "8150037902";
                    context.Companies!.Add(company);
                }
                context.SaveChanges();
                RoleManager<IdentityRole>? roleManager = app.Services.CreateScope().ServiceProvider.GetService<RoleManager<IdentityRole>>();
                if (roleManager != null)
                {
                    if (roleManager.Roles.Count() == 0)
                    {
                        identityRole = new IdentityRole("Administrator");
                        roleManager.CreateAsync(identityRole).Wait();
                        identityRole = new IdentityRole("CompanyAdministrator");
                        roleManager.CreateAsync(identityRole).Wait();
                        identityRole = new IdentityRole("RestaurantAdministrator");
                        roleManager.CreateAsync(identityRole).Wait();
                    }
                }
                UserManager<ApplicationUser>? userManager = app.Services.CreateScope().ServiceProvider.GetService<UserManager<ApplicationUser>>();
                if (userManager != null)
                {
                    if (userManager.Users.Count() == 0)
                    {
                        if (company != null)
                        {
                            applicationUser = new ApplicationUser();
                            applicationUser.UserName = "Administrator";
                            applicationUser.Name = "TAB GIDA";
                            applicationUser.Email = "abc@tabgıda.com";
                            applicationUser.PhoneNumber = "02123106600";
                            applicationUser.RegisterDate = DateTime.Today;
                            applicationUser.CompanyId = company.Id;
                            applicationUser.StateId = 1;
                            userManager.CreateAsync(applicationUser, "Admin123!").Wait();
                            userManager.AddToRoleAsync(applicationUser, "Administrator").Wait();
                        }
                    }
                }
            }

        }
        app.Run();
    }
}