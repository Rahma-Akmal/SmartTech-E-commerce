using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
	public class OrderDetails
	{
		public int Id { get; set; }
		[ForeignKey("OrderHeaders")]
		public int OrderHeadersId { get; set; }
		[ValidateNever]
		public OrderHeaders OrderHeaders { get; set; }
		public int ProductId { get; set; }
		[ValidateNever]

		public Product Product { get; set; }
		public int Count { get; set; }
		public decimal Price { get; set; }

	}
}
