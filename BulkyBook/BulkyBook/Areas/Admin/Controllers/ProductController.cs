using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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

        public async Task<IActionResult> Upsert(int ? id) //Create and Edit Poduct
        {
            IEnumerable<Category> Catlist = await _unitOfWork.Category.GetAllAsync();
            ProductVm productVm = new ProductVm()
            {
                Product = new Product(),
                CategoryList = Catlist.Select(i => new SelectListItem {
                    Text = i.Name,
                    Value = i.Id.ToString()

                }),
                CoverTypeList = _unitOfWork.CoverType.GetAll().Select(c => new SelectListItem {
                    Text = c.Name,
                    Value = c.Id.ToString()

                })

            };


            if (id==null)
            {
                //create
                return View(productVm);
            }

            // Edit
            productVm.Product= _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVm.Product ==null)
            {
                return NotFound();
            }
            return View(productVm);
            
        }

        [HttpPost]
     [ValidateAntiForgeryToken]
        public async Task <IActionResult> Upsert(ProductVm productVm)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;  //retrieve all the files that we were uploaded
                if (files.Count > 0) // File was uploaded
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\products");
                    var extenstion = Path.GetExtension(files[0].FileName);
                    if (productVm.Product.ImageUrl != null)
                    {
                        //Edit and we need to remove old images
                       
                        var imagePath = Path.Combine(webRootPath, productVm.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    //Upload new File
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extenstion), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }
                    productVm.Product.ImageUrl = @"\images\products\" + fileName + extenstion;
                }
                else
                {
                    //update when they do not change the image
                    if (productVm.Product.Id !=0)
                    {
                        Product objFromDb = _unitOfWork.Product.Get(productVm.Product.Id);
                        productVm.Product.ImageUrl = objFromDb.ImageUrl;
                    }
                }

                if (productVm.Product.Id==0)
                {
                    _unitOfWork.Product.Add(productVm.Product);                  
                 
                }
                else
                {
                    _unitOfWork.Product.Update(productVm.Product);
                }
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index)); // Ignore Magic Strings

            }

            //Common Error Handling If every property contains................
            else
            {
                IEnumerable<Category> Catlist = await _unitOfWork.Category.GetAllAsync();
                productVm.CategoryList = Catlist.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()

                });
                productVm.CoverTypeList = _unitOfWork.CoverType.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()

                });

                if (productVm.Product.Id !=0)
                {
                    productVm.Product = _unitOfWork.Product.Get(productVm.Product.Id);
                }
            };
            // ---------Common Error handling Ends-----------------------
            return View(productVm); // If every poperty contains null and If There is no any Client side validation in JS then it will show an error 
                                    // (InvalidOperationException: The ViewData item that has the key 'Product.CategoryId' is
                                    // of type 'System.Int32' but must be of type 'IEnumerable<SelectListItem>'.)
        }


        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return Json(new { data = allObj });
        }



        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Product.Get(id);

            if (objFromDb==null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            //-------Image Delete From wwwroot folder
            string webRootPath = _webHostEnvironment.WebRootPath;
            var imagePath = Path.Combine(webRootPath, objFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _unitOfWork.Product.Remove(objFromDb);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successfull" });
            
        }


        #endregion
    }
}
