using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<User>> GetAll()
        {
            return await ctx.Users.ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            return await ctx.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> AddItem(User item)
        {
            ctx.Users.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<User> UpdateItem(int id, User user)
        {
            var u = await ctx.Users.FirstOrDefaultAsync(x => x.Id == id);
            u.Name = user.Name;
            u.Phone = user.Phone;
            u.Email = user.Email;
            u.PasswordHash = user.PasswordHash;
            await ctx.Save();
            return u;
        }

        public async Task DeleteItem(int id)
        {
            var user = await ctx.Users.FirstOrDefaultAsync(x => x.Id == id);
            ctx.Users.Remove(user);
            await ctx.Save();
        }
    }
}
