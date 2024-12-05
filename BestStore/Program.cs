using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using sib_api_v3_sdk.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>( options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

//Adding Identity services to the container
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
    options => 
    {
        options.Password.RequiredLength = 6; //Min length of the password
        options.Password.RequireNonAlphanumeric = false; //No special character requirements
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//Adding the new Email service(Concept called depandancy injection) learn it too
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//Create the roles and first admin user if not availabe
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService(typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>;
    var roleManager = scope.ServiceProvider.GetRequiredService(typeof(RoleManager<IdentityRole>)) as RoleManager<IdentityRole>;

    await DatabaseInitializer.SeedDateAsync(userManager, roleManager);
}

//Since the above method is called here, it will only run once

    app.Run();
