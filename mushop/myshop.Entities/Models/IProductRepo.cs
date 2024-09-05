using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public interface IProductRepo:IGenericRepo<Product>
    {
        void Update(Product product);

    }
}
