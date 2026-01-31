using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public UserAction AddItem(UserAction item)
        {
            ctx.UserActions.Add(item);
            ctx.Save();
            return item;
        }

        public void DeleteItem(int id)
        {
            var userAction = ctx.UserActions.FirstOrDefault(x => x.Id == id);
            ctx.UserActions.Remove(userAction);
            ctx.Save();
        }

        public List<UserAction> GetAll()
        {
            return ctx.UserActions.ToList();
        }

        public UserAction GetById(int id)
        {
            return ctx.UserActions.FirstOrDefault(x => x.Id == id);
        }

        public UserAction UpdateItem(int id, UserAction userAction)
        {
            var ua = ctx.UserActions.FirstOrDefault(x => x.Id == id);
            ua.UserId = userAction.UserId;
            ua.ActionType = userAction.ActionType;
            ua.RecipeId = userAction.RecipeId;
            ua.Category = userAction.Category;
            ua.Content = userAction.Content;
            ua.Rating = userAction.Rating;
            ua.CreatedAt = userAction.CreatedAt;
            ua.Id = id;
            ctx.Save();
            return ua;
        }
    }
}
