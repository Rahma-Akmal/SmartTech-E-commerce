using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Entities.Models
{
    public interface IGenericRepo<T> where T : class
    {
        IEnumerable<T> GetAll(Expression<Func<T,bool>> ?predicate=null,string? includeword = null);
        T GetFirstOrDefault(Expression<Func<T, bool>>? predicate = null, string? includeword = null);
        void Add(T entitiy);
        void Remove(T entitiy);
        void RemoveRange(IEnumerable<T> entities);
    }
}
