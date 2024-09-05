using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
    public class CategoryRepo : GenericRepo<Category>,ICategoryRepo
    {
        private readonly ApplicationDbContext _context;
        public CategoryRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category category)
        {
            var categorydb = _context.Categories.FirstOrDefault(x => x.Id == category.Id);
            if (category != null)
            { 
                categorydb.Name= category.Name;
                categorydb.Description= category.Description;
                categorydb.CreatedDate= DateTime.Now;
            }
        }
    }
}
