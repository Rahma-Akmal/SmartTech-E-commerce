using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public interface ICategoryRepo:IGenericRepo<Category>
    {
        void Update(Category category);
    }
}
