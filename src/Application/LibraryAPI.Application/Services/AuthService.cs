using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Application.DTOs;
using LibraryAPI.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

using LibraryAPI.Domain.Entities;
using LibraryAPI.Domain.Settings;
using LibraryAPI.Domain.Enums;
using Microsoft.Extensions.Options;

namespace LibraryAPI.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IEmailService _emailService;
        private readonly FrontendSettings _frontendSettings;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly ITemplateEngineService _templateEngineService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IEmailService emailService,
            IOptions<FrontendSettings> frontendSettings,
            IEmailTemplateService emailTemplateService,
            ITemplateEngineService templateEngineService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _emailService = emailService;
            _frontendSettings = frontendSettings.Value;
            _emailTemplateService = emailTemplateService;
            _templateEngineService = templateEngineService;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserRegisterDto registerDto)
        {
            // Check email
            var existingByEmail = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingByEmail != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "El correo electrónico ya está en uso."
                };
            }

            // Check username
            var existingByName = await _userManager.FindByNameAsync(registerDto.Email);
            if (existingByName != null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "El nombre de usuario ya está en uso."
                };
            }

            var user = new ApplicationUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al completar el registro.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Assign default role
            await _userManager.AddToRoleAsync(user, "Empleado");

            // Generate Email Confirmation Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Generate link dynamically from settings
            var frontendUrl = _frontendSettings.GetConfirmEmailUrl();
            var confirmationLink = $"{frontendUrl}?email={user.Email}&token={Uri.EscapeDataString(token)}";

            // Usar la plantilla EmailConfirmation al registrarse
            var template = await _emailTemplateService.GetActiveTemplateAsync(EmailTemplateType.EmailConfirmation, user.BranchId);
            string emailBody;
            string subject;

            if (template != null)
            {
                var variables = new Dictionary<string, string>
                {
                    { "ConfirmLink", confirmationLink },
                    { "Token", token },
                    { "UserName", user.FirstName },
                    { "LibraryName", user.Branch?.Name ?? "LibraryApp" }
                };
                emailBody = _templateEngineService.RenderTemplate(template.HtmlContent, variables);
                subject = _templateEngineService.RenderTemplate(template.Subject, variables);
            }
            else
            {
                subject = "Confirma tu cuenta";
                emailBody = $@"
                    <h1>Bienvenido a LibraryApp</h1>
                    <p>Gracias por registrarte. Por favor, confirma tu correo electrónico haciendo clic en el siguiente enlace:</p>
                    <a href='{confirmationLink}'>Confirmar Mi Correo</a>
                    <br/>
                    <p>O utiliza el siguiente token directamente: <strong>{token}</strong></p>
                ";
            }

            await _emailService.SendEmailAsync(user.Email, subject, emailBody, true);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "User registered successfully. Please check your email to confirm your account."
            };
        }

        public async Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid credentials"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Su cuenta está inactiva. Por favor contacte al administrador."
                };
            }

            // Get roles
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email!, roles, user.BranchId);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = token
            };
        }
        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles.ToList()
            };
        }

        public async Task<AuthResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "User not found" };
            }

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Failed to update profile details",
                    Errors = updateResult.Errors.Select(e => e.Description).ToList()
                };
            }

            if (!string.IsNullOrEmpty(updateDto.CurrentPassword) && !string.IsNullOrEmpty(updateDto.NewPassword))
            {
                var pwdResult = await _userManager.ChangePasswordAsync(user, updateDto.CurrentPassword, updateDto.NewPassword);
                if (!pwdResult.Succeeded)
                {
                    return new AuthResponseDto
                    {
                        IsSuccess = false,
                        Message = "Failed to update password",
                        Errors = pwdResult.Errors.Select(e => e.Description).ToList()
                    };
                }
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Profile updated successfully" };
        }

        public async Task<AuthResponseDto> ConfirmEmailAsync(ConfirmEmailDto confirmDto)
        {
            var user = await _userManager.FindByEmailAsync(confirmDto.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Usuario no encontrado." };
            }

            var result = await _userManager.ConfirmEmailAsync(user, confirmDto.Token);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al confirmar el correo electrónico. El token es inválido o expiró.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            // Enviar correo de Bienvenida (Welcome) después de confirmar exitosamente
            var template = await _emailTemplateService.GetActiveTemplateAsync(EmailTemplateType.Welcome, user.BranchId);
            string emailBody;
            string subject;
            var loginLink = $"{_frontendSettings.BaseUrl}/login";

            if (template != null)
            {
                var variables = new Dictionary<string, string>
                {
                    { "LoginLink", loginLink },
                    { "UserName", user.FirstName },
                    { "LibraryName", user.Branch?.Name ?? "LibraryApp" }
                };
                emailBody = _templateEngineService.RenderTemplate(template.HtmlContent, variables);
                subject = _templateEngineService.RenderTemplate(template.Subject, variables);
            }
            else
            {
                subject = "¡Bienvenido a LibraryApp!";
                emailBody = $@"
                    <h1>¡Bienvenido, {user.FirstName}!</h1>
                    <p>Tu cuenta ha sido confirmada exitosamente. Ya puedes iniciar sesión y comenzar a utilizar el sistema.</p>
                    <a href='{loginLink}'>Ir al Inicio de Sesión</a>
                ";
            }

            await _emailService.SendEmailAsync(user.Email, subject, emailBody, true);

            return new AuthResponseDto { IsSuccess = true, Message = "Correo confirmado exitosamente." };
        }

        public async Task<AuthResponseDto> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                // Por cuestiones de seguridad, no se revela si el usuario existe o no
                return new AuthResponseDto { IsSuccess = true, Message = "Si el correo está registrado, se han enviado instrucciones para restablecer la contraseña." };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var frontendUrl = _frontendSettings.GetResetPasswordUrl();
            var resetLink = $"{frontendUrl}?email={user.Email}&token={Uri.EscapeDataString(token)}";

            var template = await _emailTemplateService.GetActiveTemplateAsync(EmailTemplateType.PasswordReset, user.BranchId);
            string emailBody;
            string subject;

            if (template != null)
            {
                var variables = new Dictionary<string, string>
                {
                    { "ResetLink", resetLink },
                    { "Token", token },
                    { "UserName", user.FirstName },
                    { "LibraryName", user.Branch?.Name ?? "LibraryApp" }
                };
                emailBody = _templateEngineService.RenderTemplate(template.HtmlContent, variables);
                subject = _templateEngineService.RenderTemplate(template.Subject, variables);
            }
            else
            {
                subject = "Restablecimiento de Contraseña";
                emailBody = $@"
                    <h1>Restablecer Contraseña</h1>
                    <p>Has solicitado restablecer tu contraseña. Haz clic en el enlace para continuar:</p>
                    <a href='{resetLink}'>Restablecer Contraseña</a>
                    <br/>
                    <p>O utiliza el siguiente token directamente: <strong>{token}</strong></p>
                    <p>Si no fuiste tú, puedes ignorar este correo.</p>
                ";
            }

            await _emailService.SendEmailAsync(user.Email, subject, emailBody, true);

            return new AuthResponseDto { IsSuccess = true, Message = "Si el correo está registrado, se han enviado instrucciones para restablecer la contraseña." };
        }

        public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                // Seguridad
                return new AuthResponseDto { IsSuccess = false, Message = "Error al intentar restablecer la contraseña." };
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Error al restablecer la contraseña. El token es inválido o expiró.",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                };
            }

            return new AuthResponseDto { IsSuccess = true, Message = "Contraseña restablecida exitosamente." };
        }
    }
}
