using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.DataAccess.Implementation
{
	public class OrderHeaderRepo:GenericRepo<OrderHeaders>, IOrderHeaderRepo
	{
		private readonly ApplicationDbContext _context;
		public OrderHeaderRepo(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
		public void Update(OrderHeaders orderHeaders)
		{
			_context.OrderHeaders.Update(orderHeaders);
		}

		public void UpdateStatus(int id, string OrderStatus, string PaymentStatus)
		{
			var orderdb= _context.OrderHeaders.FirstOrDefault(x=>x.Id==id);
			if (orderdb != null)
			{ 
				orderdb.OrderStatus=OrderStatus;
                orderdb.PaymentDate = DateTime.Now;
                if (PaymentStatus != null)
				{
					orderdb.PaymentStatus=PaymentStatus;
					
				}
			}
		}
	}
}
