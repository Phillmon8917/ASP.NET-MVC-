using BestStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BestStore.Controllers
{
	[Authorize(Roles = "admin")]
	[Route("/Admin/[controller]/{action=Index}/{id?}")]
	public class UsersController : Controller
	{
        //read the registered user
		public readonly UserManager<ApplicationUser> userManager;
		public readonly RoleManager<IdentityRole> roleManager;
		public readonly int pageSize = 5;
        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
			this.userManager = userManager;
			this.roleManager = roleManager;
        }

        public IActionResult Index(int? pageIndex)
		{
			IQueryable<ApplicationUser> query = userManager.Users.OrderByDescending(u => u.CreatedAt);

			//Pagination Fucntionality
			if (pageIndex == null || pageIndex < 1)
			{
				pageIndex = 1;
			}

			decimal count = query.Count();
			int totalPages = (int)Math.Ceiling(count / pageSize);
			query = query.Skip(((int)pageIndex - 1) * pageSize).Take(pageSize);

			var users = query.ToList();

			//Adding the pages to viewbage so we can display thier buttons
			ViewBag.PageIndex = pageIndex;
			ViewBag.TotalPages = totalPages;

			return View(users);
		}

		//Details Action of the User
		public async Task<IActionResult> Details(string? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Users");
			}

			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null)
			{
				return RedirectToAction("Index", "Users");
			}

			ViewBag.Roles = await userManager.GetRolesAsync(appUser);

			//Get the available roles
			var availableRoles = roleManager.Roles.ToList();
			var items = new List<SelectListItem>();
			foreach (var role in availableRoles)
			{
				items.Add(new SelectListItem
				{
					Text = role.NormalizedName,
					Value = role.Name,
					Selected = await userManager.IsInRoleAsync(appUser, role.Name!)
				});
			}

			ViewBag.SelectItems = items;

			return View(appUser);
		}

		//Action to update roles
		public async Task<IActionResult> EditRole(string? id, string? newRole)
		{
			if (id == null || newRole == null)
			{
				return RedirectToAction("Index", "Users");
			}

			//check if role and user exists
			var roleExists = await roleManager.RoleExistsAsync(newRole);
			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null || roleExists == null)
			{
				return RedirectToAction("Index", "Users");
			}

			//checking if the user is not current user(Meaning admin is not trying to edit his/role)

			var currentUser = await userManager.GetUserAsync(User);
            if ( currentUser!.Id == appUser.Id)
            {
				TempData["ErrorMessage"] = "You cannot update your own role!";
				return RedirectToAction("Details", "Users", new {id});
            }

			//Otherwise update the user roles
			var userRoles = await userManager.GetRolesAsync(appUser);
			await userManager.RemoveFromRolesAsync(appUser, userRoles);
			await userManager.AddToRoleAsync(appUser, newRole);

			TempData["SuccessMessage"] = "User role updated successfully";
			return RedirectToAction("Details", "Users", new {id});
        }

		public async Task<IActionResult> DeleteAccount(string? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Users");
			}

			var appUser = await userManager.FindByIdAsync(id);
			if (appUser == null)
			{
				return RedirectToAction("Index", "Users");
			}

			//Check if this is a current user or not
			var currentUser = await userManager.GetUserAsync (User);
			if (appUser.Id == currentUser!.Id)
			{
				TempData["ErrorMessage"] = "You cannot delete your own account!";
				return RedirectToAction("Details", "Users" , new { id });
			}

			//Otherwise delete the user Account
			var result = await userManager.DeleteAsync(appUser);
			if (result.Succeeded)
			{
				return RedirectToAction("Index", "Users");
			}

			TempData["ErrorMessage"] = "Unable to delete this account: " + result.Errors.First().Description;
			return RedirectToAction("Details", "Users", new { id });
		}
	}
}
