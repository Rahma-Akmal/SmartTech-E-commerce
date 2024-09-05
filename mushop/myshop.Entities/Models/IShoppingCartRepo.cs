using myshop.Entities.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public interface IShoppingCartRepo: IGenericRepo<ShoppingCart>
    {
        int IncreaseCount(ShoppingCart shoppingCart, int count);

        int DecreaseCount(ShoppingCart shoppingCart, int count);
    }
}
