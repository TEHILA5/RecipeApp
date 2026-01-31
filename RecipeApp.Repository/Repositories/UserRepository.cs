using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;

namespace RecipeApp.Repository.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly IContext ctx;

        public UserRepository(IContext context)
        {
            ctx = context;
        }

        public User AddItem(User item)
        {
            ctx.Users.Add(item);
            ctx.Save();
            return item;
        }

        public void DeleteItem(int id)
        {
            var user = ctx.Users.FirstOrDefault(x => x.Id == id);
            ctx.Users.Remove(user);
            ctx.Save();
        }

        public List<User> GetAll()
        {
            return ctx.Users.ToList();
        }

        public User GetById(int id)
        {
            return ctx.Users.FirstOrDefault(x => x.Id == id);
        }

        public User UpdateItem(int id, User user)
        {
            var u = ctx.Users.FirstOrDefault(x => x.Id == id);
            u.Name = user.Name;
            u.Phone = user.Phone;
            u.Email = user.Email;
            u.PasswordHash = user.PasswordHash;
            u.Id = id;
            ctx.Save();
            return u;
        }
    }
}
