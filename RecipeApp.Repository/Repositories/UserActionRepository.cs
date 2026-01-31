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
    public class UserActionRepository : IRepository<UserAction>
    {
        private readonly IContext ctx;

        public UserActionRepository(IContext context)
        {
            ctx = context;
        }

        public async Task<List<UserAction>> GetAll()
        {
            return await ctx.UserActions.ToListAsync();
        }

        public async Task<UserAction> GetById(int id)
        {
            return await ctx.UserActions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<UserAction> AddItem(UserAction item)
        {
            ctx.UserActions.Add(item);
            await ctx.Save();
            return item;
        }

        public async Task<UserAction> UpdateItem(int id, UserAction userAction)
        {
            var ua = await ctx.UserActions.FirstOrDefaultAsync(x => x.Id == id);
            ua.UserId = userAction.UserId;
            ua.ActionType = userAction.ActionType;
            ua.RecipeId = userAction.RecipeId;
            ua.Category = userAction.Category;
            ua.Content = userAction.Content;
            ua.Rating = userAction.Rating;
            ua.CreatedAt = userAction.CreatedAt;
            await ctx.Save();
            return ua;
        }

        public async Task DeleteItem(int id)
        {
            var userAction = await ctx.UserActions.FirstOrDefaultAsync(x => x.Id == id);
            ctx.UserActions.Remove(userAction);
            await ctx.Save();
        }
    }
}
