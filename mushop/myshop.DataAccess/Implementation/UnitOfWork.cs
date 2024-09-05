using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
    public class UnitOfWork : IUnitOfWork

    {
        private readonly ApplicationDbContext _context;
        public ICategoryRepo Category { get; private set; }

        public IProductRepo Product { get; private set; }
        public IShoppingCartRepo ShoppingCart { get; private set; }
        public IOrderHeaderRepo OrderHeader { get; private set; }
        public IOrderDetailsRepo OrderDetails { get; private set; }
        public IApplicationUserRepo ApplicationUser { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Category=new CategoryRepo(context);
            Product = new ProductRepo(context);
            ShoppingCart = new ShoppingCartRepo(context);
            OrderHeader = new OrderHeaderRepo(context);
            OrderDetails = new OrderDetailsRepo(context);
            ApplicationUser=new ApplicationUserRepo(context);

        }


        public int Complete()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
