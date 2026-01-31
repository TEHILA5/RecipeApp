using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs; 

namespace RecipeApp.Services.Interfaces
{
    public interface IUserService : IService<UserDto>
    { 
        Task<UserDto> Login(UserLoginDto loginDto);
        Task<UserDto> Register(UserCreateDto createDto);
        Task<bool> EmailExists(string email);
    }
}
