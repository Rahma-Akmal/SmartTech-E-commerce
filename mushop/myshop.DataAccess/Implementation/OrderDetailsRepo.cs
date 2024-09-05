using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
	public class OrderDetailsRepo:GenericRepo<OrderDetails>,IOrderDetailsRepo
	{
		private readonly ApplicationDbContext _context;
		public OrderDetailsRepo(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public void Update(OrderDetails orderDetails)
		{
			_context.OrderDetails.Update(orderDetails);
		}
	}
}
