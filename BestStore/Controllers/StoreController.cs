using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Mvc;

namespace BestStore.Controllers
{
	public class StoreController : Controller
	{
		private readonly ApplicationDbContext context;
		private readonly int pageSize = 4;
        public StoreController(ApplicationDbContext context)
        {
			this.context = context;
        }
        public IActionResult Index(int pageIndex,string? search,string? brand, string? sort)
		{
			IQueryable<Product> query = context.Products;

			//Search functionallity
			if (search != null && search.Length > 0)
			{
				query = query.Where(p => p.Name.Contains(search));
			}

			//filter functionallity
			if(brand != null && brand.Length > 0)
			{
				query = query.Where(p => p.Brand.Contains(brand));
			}

			if(sort == "price_asc")
			{
				query = query.OrderBy(p => p.Price);
			}
			else if(sort == "price_desc")
			{
				query = query.OrderByDescending(p => p.Price);
			}
			else 
			{
                query = query.OrderByDescending(p => p.Id);
            }

			// Pagination function
			if (pageIndex < 1)
			{
				pageIndex = 1;
			}

			decimal count = query.Count();
			int totalPages = (int)Math.Ceiling(count / pageSize);
			query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

			var products = query.ToList();
			ViewBag.Products = products;
			ViewBag.TotalPages = totalPages;
			ViewBag.PageIndex = pageIndex;

            StoreSearchModel storeSearchModel = new StoreSearchModel()
			{
				Brand = brand,
			    Search = search,
			    Sort = sort
			};

			return View(storeSearchModel);
		}

		public IActionResult Details(int id)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index", "Store");
			}

			return View(product);
		}
	}
}
