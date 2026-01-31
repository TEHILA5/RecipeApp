using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipeApp.Repository.Interfaces
{
    public interface IRepository<T>
    {
        List<T> GetAll();
        T GetById(int id);
        T AddItem(T item);
        T UpdateItem(int id, T item);
        void DeleteItem(int id);
    }
}
