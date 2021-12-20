using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBook.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {          
            return View();
        }

        public IActionResult Upsert(int ? id)
        {
            Category category = new Category();
            if (id==null)
            {
                //create
                return View(category);
            }
            //this is for Edit
            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if (category==null)
            {
                return NotFound();
            }
            return View(category);
            
        }



        #region API Calls

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Category.GetAll();
            return Json(new {data=allObj });
        }


        #endregion
    }
}
