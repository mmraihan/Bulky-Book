using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly  IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;// Upload images on the server in a folder inside

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {          
            return View();
        }

        public IActionResult Upsert(int ? id)
        {
            Product product = new Product();
            if (id==null)
            {
                //create
                return View(product);
            }
            //this is for Edit
            product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (product==null)
            {
                return NotFound();
            }
            return View(product);
            
        }

        [HttpPost]
     [ValidateAntiForgeryToken]
        public IActionResult Upsert(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.Id==0)
                {
                    _unitOfWork.Product.Add(product);
                    
                 
                }
                else
                {
                    _unitOfWork.Product.Update(product);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index)); // Ignore Magic Strings

            }
            return View(product);
        }


        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);

            if (objFromDb==null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successfull" });
            
        }


        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll();
            return Json(new {data=allObj });
        }


        #endregion
    }
}
