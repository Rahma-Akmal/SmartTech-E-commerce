﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
	public interface IOrderHeaderRepo:IGenericRepo<OrderHeaders>
	{
		void Update(OrderHeaders orderHeaders);
		void UpdateStatus(int id,string OrderStatus,string PaymentStatus);


	}
}