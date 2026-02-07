using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs; 

namespace RecipeApp.Services.Interfaces
{
    public interface IUserService 
    { 
        Task<List<UserAdminDto>> GetAll();
        Task<UserAdminDto> GetById(int id);
        Task<UserAdminDto> UpdateItem(int id, UserAdminDto item);
        Task DeleteItem(int id);
        Task<UserAdminDto> Register(UserCreateDto createDto);
        Task<UserAdminDto> Login(UserLoginDto loginDto);
        Task<bool> EmailExists(string email);
    }
}
