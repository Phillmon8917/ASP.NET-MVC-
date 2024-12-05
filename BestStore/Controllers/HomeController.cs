using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BestStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext Context;

        public HomeController(ApplicationDbContext context)
        {
           this.Context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult News()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
    }
}
