using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RecipeApp.Common.DTOs;
using RecipeApp.DataContext;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace RecipeApp.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IMapper _mapper;

        public UserService(IRepository<User> userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        //Generic CRUD
        public async Task<List<UserAdminDto>> GetAll()
        {
            var users = await _userRepository.GetAll();
            return _mapper.Map<List<UserAdminDto>>(users);
        }

        public async Task<UserAdminDto> GetById(int id)
        {
            var user = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");
            return _mapper.Map<UserAdminDto>(user);
        }

        public async Task<UserAdminDto> UpdateItem(int id, UserAdminDto item)
        {
            var existing = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(item.Name))
                existing.Name = item.Name;

            if (!string.IsNullOrWhiteSpace(item.Phone))
                existing.Phone = item.Phone;

            // בדיקת אימייל אם השתנה
            if (!string.IsNullOrEmpty(item.Email) &&
                !string.Equals(item.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                // בדיקה שהאימייל החדש לא קיים אצל משתמש אחר
                if (await EmailExists(item.Email))
                    throw new InvalidOperationException("Email already exists.");

                existing.Email = item.Email;
            }

            var updated = await _userRepository.UpdateItem(id, existing);
            return _mapper.Map<UserAdminDto>(updated);
        }

        public async Task DeleteItem(int id)
        {
            var existing = await _userRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"User with id {id} not found.");
            await _userRepository.DeleteItem(id);
        }

        //   User-Specific  
        public async Task<UserAdminDto> Register(UserCreateDto createDto)
        { 
            if (await EmailExists(createDto.Email))
                throw new InvalidOperationException("Email already exists.");

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = HashPassword(createDto.Password);
            user.CreatedAt = DateTime.UtcNow;

            var created = await _userRepository.AddItem(user);
            return _mapper.Map<UserAdminDto>(created);
        }

        public async Task<UserAdminDto> Login(UserLoginDto loginDto)
        {
            var users = await _userRepository.GetAll();
            var user = users.FirstOrDefault(u =>
                string.Equals(u.Email, loginDto.Email, StringComparison.OrdinalIgnoreCase))
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return _mapper.Map<UserAdminDto>(user);
        }

        public async Task<bool> EmailExists(string email)
        {
            var users = await _userRepository.GetAll();
            return users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }

        //  Password Hashing  
        // הצפנת סיסמה

        private static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(salt);

            string hash = Convert.ToBase64String(DeriveKey(password, salt));
            string saltBase64 = Convert.ToBase64String(salt);
            return $"{saltBase64}:{hash}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string actualHash = Convert.ToBase64String(DeriveKey(password, salt));

            return actualHash == parts[1];
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            return KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32);
        }
    }
}
