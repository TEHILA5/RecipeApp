using AutoMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RecipeApp.Common.DTOs;
using RecipeApp.Repository.Entities;
using RecipeApp.Repository.Interfaces;
using RecipeApp.Services.Interfaces;

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


        public async Task<UserAdminDto> UpdateMe(int id, UserUpdateDto dto)
        {
            var existing = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                existing.Phone = dto.Phone;

            if (!string.IsNullOrEmpty(dto.Email) &&
                !string.Equals(dto.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await EmailExists(dto.Email))
                    throw new InvalidOperationException("Email already exists.");
                existing.Email = dto.Email;
            }

            var updated = await _userRepository.UpdateItem(id, existing);
            return _mapper.Map<UserAdminDto>(updated);
        }

        public async Task<UserAdminDto> UpdateUser(int id, UserAdminUpdateDto dto)
        {
            var existing = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                existing.Phone = dto.Phone;

            if (!string.IsNullOrEmpty(dto.Email) &&
                !string.Equals(dto.Email, existing.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await EmailExists(dto.Email))
                    throw new InvalidOperationException("Email already exists.");
                existing.Email = dto.Email;
            }

            var updated = await _userRepository.UpdateItem(id, existing);
            return _mapper.Map<UserAdminDto>(updated);
        }

        public async Task DeleteItem(int id)
        {
            _ = await _userRepository.GetById(id)
                ?? throw new KeyNotFoundException($"User with id {id} not found.");
            await _userRepository.DeleteItem(id);
        }

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

        public async Task ResetPassword(ResetPasswordDto resetDto)
        {
            var users = await _userRepository.GetAll();
            var user = users.FirstOrDefault(u =>
                string.Equals(u.Email, resetDto.Email, StringComparison.OrdinalIgnoreCase))
                ?? throw new KeyNotFoundException("No account found with this email address.");

            if (string.IsNullOrWhiteSpace(resetDto.NewPassword) || resetDto.NewPassword.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters.");

            user.PasswordHash = HashPassword(resetDto.NewPassword);
            await _userRepository.UpdateItem(user.Id, user);
        }

        private async Task<bool> EmailExists(string email)
        {
            var users = await _userRepository.GetAll();
            return users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        }

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