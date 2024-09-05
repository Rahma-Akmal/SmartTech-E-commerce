using myshop.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.ViewModel
{
    public class OrderVM
    {
        public OrderHeaders orderHeaders {  get; set; }
        public IEnumerable<OrderDetails> orderDetails { get; set; }
    }
}
