using PlanIT.BusinessLogic.DTOs;
using PlanIT.BusinessLogic.Interfaces;
using PlanIT.Domain;
using PlanIT.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PlanIT.BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;
        private readonly IEmailService _emailService;


        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtProvider JwtProvider, IEmailService emailService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtProvider = JwtProvider;
            _emailService = emailService;
        }

        public async Task RegisterAsync(UserRegisterDto dto)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ArgumentException("El email ya está en uso.");

            // Hashear la contraseña, la contraseña no se guarda
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Crear nuevo usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                PasswordHash = passwordHash
            };

            // Guardar en la base de datos
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> LoginAsync(UserLoginDto dto)
        {
            // Buscar al usuario
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new ArgumentException("Email o contraseña incorrectos.");

            // Verificar la contraseña
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isValidPassword)
                throw new ArgumentException("Email o contraseña incorrectos.");

            // 3. Generar el Token JWT (simple para test)
            string token = _jwtProvider.GenerateToken(user);
            
            return token;
        }

        // Implementación de los nuevos métodos para el restablecimiento de contraseña
        public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null)
                throw new ArgumentException("El email no está registrado");

            // Generar un token 
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.PasswordResetToken = token;
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1); // Token por 1 hora
            
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || user.PasswordResetToken != dto.Token || user.PasswordResetExpires < DateTime.UtcNow)
                throw new ArgumentException("Token inválido o expirado.");

            // Hashear la nueva contraseña
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpires = null;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}