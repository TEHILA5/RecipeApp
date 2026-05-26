using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipeApp.Common.DTOs; 

namespace RecipeApp.Services.Interfaces
{
    public interface IUserService:IService<UserAdminDto>
    { 
        Task<UserAdminDto> Register(UserCreateDto createDto);
        Task<UserAdminDto> Login(UserLoginDto loginDto);
        Task ResetPassword(ResetPasswordDto resetDto);
        Task<UserAdminDto> UpdateMe(int id, UserUpdateDto dto);
        Task<UserAdminDto> UpdateUser(int id, UserAdminUpdateDto dto);
    }
}
