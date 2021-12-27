
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BulkyBook.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties:"Category,CoverType");
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            var productFromDb = _unitOfWork.Product
                .GetFirstOrDefault(u => u.Id == id, includeProperties: "Category,CoverType");
         

            ShoppingCart cartObj = new ShoppingCart()
            {
                Product=productFromDb,
                ProductId=productFromDb.Id
            };
            return View(cartObj);

        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart CartObject)
        {
            CartObject.Id = 0;
            if (ModelState.IsValid)
            {
                //Theb we will add to cart

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.GetFirstOrDefault(
                    u => u.ApplicationUserId == CartObject.ApplicationUserId && u.ProductId==CartObject.ProductId,
                    includeProperties:("Product")
                    );

                if (cartFromDb==null)
                {
                    //No records exists in db for that user, we need to add shoppingCart

                    _unitOfWork.ShoppingCart.Add(CartObject);

                }
                else
                {
                    cartFromDb.Count += CartObject.Count; //Whenever we addtoCart in multipletimes
                    _unitOfWork.ShoppingCart.Update(cartFromDb); //if we don't write, it will also work, because EF tracks the data
                }

                _unitOfWork.Save();

                //--In session we will store number of items in the shopping cart---

                #region In session we will store number of items in the shopping car

                var count = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == CartObject.ApplicationUserId)
                   .ToList().Count();

                //HttpContext.Session.SetObject(SD.ssShoppingCart, CartObject); //------These are custom session for more than integer
                //var obj = HttpContext.Session.GetObject<ShoppingCart>(SD.ssShoppingCart); //------These are custom session

                //Deafult Session
                HttpContext.Session.SetInt32(SD.ssShoppingCart, count);

                #endregion



                return RedirectToAction(nameof(Index));


            }
            else //Same prodcut will return back
            {
                var productFromDb = _unitOfWork.Product
             .GetFirstOrDefault(u => u.Id == CartObject.ProductId, includeProperties: "Category,CoverType");


                ShoppingCart cartObj = new ShoppingCart()
                {
                    Product = productFromDb,
                    ProductId = productFromDb.Id
                };
                return View(cartObj);
            }

         
           

        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
