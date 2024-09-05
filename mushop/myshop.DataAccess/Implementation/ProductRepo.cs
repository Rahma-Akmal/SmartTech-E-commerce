using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
    public class ProductRepo: GenericRepo<Product>, IProductRepo
    {
        private readonly ApplicationDbContext _context;
        public ProductRepo(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Product product)
        {
            var productdb = _context.products.FirstOrDefault(x => x.Id == product.Id);
            if (productdb != null)
            {
                productdb.Name = product.Name;
                productdb.Description = product.Description;
                productdb.Price = product.Price;
                productdb.Img = product.Img;
                productdb.CategoryId = product.CategoryId;
            }
        }
    }
}
