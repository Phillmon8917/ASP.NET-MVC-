using BestStore.Models;
using BestStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace BestStore.Controllers
{
    [Authorize(Roles = "admin")] //This protects the controller to avail it to admins only
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly int pageSize = 3;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
            IQueryable<Product> query = context.Products;

            //Search functionallity
            if( search != null)
            {
                query = query.Where(s => s.Name.Contains(search) || s.Brand.Contains(search));
            }

            //Sort Functionallity
            string[] validColumns = new string[] { "Id", "Name", "Brand", "Category", "Price", "CreatedAt" };
            string[] validOrderBy = new string[] { "desc" ,"asc" };

            //default column to Id
            if(!validColumns.Contains(column))
            {
                column = "Id";
            }

            //Default orderby to desc
            if(!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }

            //query.OrderByDescending(p => p.Id);
            //orderby descending
            Func<IQueryable<Product>, IQueryable<Product>> orderByFunc;
            var columnMapping = new Dictionary<string, Func<IQueryable<Product>, bool, IQueryable<Product>>>
            {
               { "Name", (q, isAsc) => isAsc ? q.OrderBy(p => p.Name) : q.OrderByDescending(p => p.Name) },
               { "Brand", (q, isAsc) => isAsc ? q.OrderBy(p => p.Brand) : q.OrderByDescending(p => p.Brand) },
               { "Category", (q, isAsc) => isAsc ? q.OrderBy(p => p.Category) : q.OrderByDescending(p => p.Category) },
               { "Price", (q, isAsc) => isAsc ? q.OrderBy(p => p.Price) : q.OrderByDescending(p => p.Price) },
               { "CreatedAt", (q, isAsc) => isAsc ? q.OrderBy(p => p.CreatedAt) : q.OrderByDescending(p => p.CreatedAt) }
            };

            if (columnMapping.TryGetValue(column, out var sortFunc))
            {
                query = sortFunc(query, orderBy.Equals("asc"));
            }
            else
            {
                query = orderBy.Equals("asc") ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
            }

            //Pagination Functionallity
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            

            //get the number of products in the database
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);// ceiling takes decimal next in e.g 3.5 to 4
            query = query.Skip((pageIndex - 1 ) * pageSize).Take(pageSize);

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            //Adding orderby and column to viewdata dictionary so we can access them
            ViewData["OrderBy"] = orderBy;
            ViewData["Column"] = column;

            //Save search to ViewData else if its null take empty string
            ViewData["Search"] = search ?? "";

            var products = query.ToList();
            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ProductDto productDto)
        { 
            if(productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if(!ModelState.IsValid)
            {
                return View(productDto);
            }

            //Save the Image file
            string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

            string imageFullPath = environment.WebRootPath + "/Images/" + newFileName;
            using(var stream = System.IO.File.Create(imageFullPath))
            {
                productDto.ImageFile.CopyTo(stream);
            }

            //Saving the created Product in the database
            Product product = new Product()
            {
                Name = productDto.Name,
                Brand = productDto.Brand,
                Category = productDto.Category,
                Price = productDto.Price,
                Description = productDto.Description,
                ImageFileName = newFileName,
                CreatedAt = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }

        public ActionResult Edit(int id)
        {
            var product = context.Products.Find(id);

            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            //Create productDto from Product
            ProductDto productDto = new ProductDto()
            {
                Brand = product.Brand,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Description = product.Description,
                
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageFileName;
            ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MM/yyyy HH:mm");

            return View(productDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ProductDto productDto)
        {
            var product = context.Products.Find(id);

            if(product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            if(!ModelState.IsValid)
            {
                ViewData["ProductId"] = product.Id;
                ViewData["ImageFileName"] = product.ImageFileName;
                ViewData["CreatedAt"] = product.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                return View(productDto);
            }

            //Update the image file if we have a new one
            string newFileName = product.ImageFileName;
            if(productDto.ImageFile != null)
            {
                //NewFileName
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                newFileName += Path.GetExtension(productDto.ImageFile.FileName);

                //Setting the path for the string
                string imageFullPath = environment.WebRootPath + "/Images/" + newFileName;

                using(var stream = System.IO.File.Create(imageFullPath))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                //Deleting the old image
                string oldImageFilePath = environment.WebRootPath + "/Images/" + product.ImageFileName;
                System.IO.File.Delete(oldImageFilePath);
            }

            //Update the product in the database

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description;
            product.ImageFileName = newFileName;

            context.SaveChanges();
            return RedirectToAction("Index", "Products");
        }

        //The delete action
        public IActionResult Delete(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return RedirectToAction("Index", "Products");
            }

            string imageFileName = environment.WebRootPath + "/Images/" + product.ImageFileName;
            System.IO.File.Delete(imageFileName);

            context.Products.Remove(product);
            context.SaveChanges();

            return RedirectToAction("Index", "Products");
        }
    }
}
