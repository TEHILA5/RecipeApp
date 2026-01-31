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
        public async Task<List<UserDto>> GetAll()
        {
            var users = await _userRepository.GetAll();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> GetById(int id)
        {
            var user = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");
            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> AddItem(UserDto item)
        {
            var user = _mapper.Map<User>(item);
            var created = await _userRepository.AddItem(user);
            return _mapper.Map<UserDto>(created);
        }

        public async Task<UserDto> UpdateItem(int id, UserDto item)
        {
            var existing = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            _mapper.Map(item, existing);
            var updated = await _userRepository.UpdateItem(id, existing);
            return _mapper.Map<UserDto>(updated);
        }

        public async Task DeleteItem(int id)
        {
            var existing = await _userRepository.GetById(id);
            if (existing == null)
                throw new KeyNotFoundException($"User with id {id} not found.");
            await _userRepository.DeleteItem(id);
        }

        //   User-Specific  
        public async Task<UserDto> Register(UserCreateDto createDto)
        {
            if (await EmailExists(createDto.Email))
                throw new InvalidOperationException("Email already exists.");

            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = HashPassword(createDto.Password);

            var created = await _userRepository.AddItem(user);
            return _mapper.Map<UserDto>(created);
        }

        public async Task<UserDto> Login(UserLoginDto loginDto)
        {
            var users = await _userRepository.GetAll();
            var user = users.FirstOrDefault(u => u.Email == loginDto.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> EmailExists(string email)
        {
            var users = await _userRepository.GetAll();
            return users.Any(u => u.Email == email);
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
