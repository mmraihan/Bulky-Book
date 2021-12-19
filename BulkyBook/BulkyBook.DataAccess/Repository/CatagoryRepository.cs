using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BulkyBook.DataAccess.Repository
{
    class CatagoryRepository : Repository<Category>, ICatagoryRepository
    {
        private readonly ApplicationDbContext _db;
        public CatagoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Category category)
        {
            var objFromDb = _db.Categories.FirstOrDefault(i => i.Id == category.Id);
            if (objFromDb !=null)
            {
                objFromDb.Name = category.Name;
                _db.SaveChanges();
            }
       
            
        }
    }
}
