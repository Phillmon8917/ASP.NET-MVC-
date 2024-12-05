using BestStore.Models;
using Microsoft.AspNetCore.Identity;

namespace BestStore.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDateAsync(UserManager<ApplicationUser>? userManager, 
            RoleManager<IdentityRole>? roleManager)
        {
            
            
            if (userManager == null || roleManager == null)
            {
                Console.WriteLine("userManager or rolemanager is null => exit");
                return;
            }
            

            //Checking if we have an admin role or not
            var exists = await roleManager.RoleExistsAsync("admin");
            if(!exists)
            {
                Console.WriteLine("Admin role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("admin")); //Creating an admin role if doesnt exist
            }

            //Checking if the sellor role exists or not
            exists = await roleManager.RoleExistsAsync("seller");
            if (!exists)
            {
                Console.WriteLine("Seller role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }

            //checking if the client role exists or not
            exists = await roleManager.RoleExistsAsync("client");
            if (!exists)
            {
                Console.WriteLine("Client role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            //checking if we have atleast one admin user or not
            var adminUser = await userManager.GetUsersInRoleAsync("admin");
            if(adminUser.Any())
            {
                Console.WriteLine("Admin user already exists =>  exit");
                return;
            }

            //Create the admin user
            var user = new ApplicationUser()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                CreatedAt = DateTime.Now,
            };

            string initialPassword = "admin@123";

            var results = await userManager.CreateAsync(user, initialPassword);
            if(results.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Admin user created successfully! Please update the initial password!");
                Console.WriteLine("Email is: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);
            }
        }
    }
}
