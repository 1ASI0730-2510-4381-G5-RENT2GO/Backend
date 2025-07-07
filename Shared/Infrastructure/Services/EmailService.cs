using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using SendGrid;
using SendGrid.Helpers.Mail;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;
using Rent2Go.API.IAM.Domain.Model;
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using Rent2Go.API.Shared.Domain.Services;

namespace BackendRent2Go.Shared.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public EmailService(IConfiguration config, ApplicationDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            // Usar únicamente SendGrid y no intentar respaldo SMTP
            await SendWithSendGridAsync(to, subject, htmlContent);
        }
        
        private async Task SendWithSendGridAsync(string to, string subject, string htmlContent)
        {
            try
            {
                // Obtener configuración de SendGrid
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ??
                             _config["SendGridSettings:ApiKey"];
                
                Console.WriteLine($"SendGrid API Key encontrada: {!string.IsNullOrEmpty(apiKey)}");
                
                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new Exception("SendGrid API Key no configurada");
                }
                
                var fromEmail = Environment.GetEnvironmentVariable("SENDGRID_FROM_EMAIL") ??
                                _config["SendGridSettings:FromEmail"] ?? "noreply@rent2go.com";
                var fromName = Environment.GetEnvironmentVariable("SENDGRID_FROM_NAME") ??
                               _config["SendGridSettings:FromName"] ?? "Rent2Go";
                
                Console.WriteLine($"Enviando correo con SendGrid desde: {fromEmail} ({fromName}) a: {to}");
                
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(fromEmail, fromName);
                var toEmail = new EmailAddress(to);
                
                // El texto plano se deja vacío ("") para que SendGrid genere automáticamente una versión de texto
                // a partir del contenido HTML
                var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, "", htmlContent);
                
                var response = await client.SendEmailAsync(msg);
                
                Console.WriteLine($"Respuesta de SendGrid: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
                    Console.WriteLine($"Error al enviar email con SendGrid: {response.StatusCode}, Detalle: {responseBody}");
                    throw new Exception($"Error al enviar email con SendGrid: {response.StatusCode}");
                }
                
                Console.WriteLine($"Email enviado correctamente a {to} usando SendGrid");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar email con SendGrid a {to}: {ex.Message}");
                // No lanzamos la excepción para evitar interrumpir el flujo de la aplicación
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetCode)
        {
            var subject = "Recuperación de contraseña - Rent2Go";
            var content = GetEmailTemplate("reset_password.html");
            
            if (string.IsNullOrEmpty(content))
            {
                // Si no encuentra la plantilla, usa una versión básica
                content = $@"
                    <h1>Recuperación de contraseña</h1>
                    <p>Usa el siguiente código para restablecer tu contraseña:</p>
                    <h2 style='color: #4CAF50; font-size: 24px;'>{resetCode}</h2>
                    <p>Este código expirará en 1 hora.</p>
                    <p>Si no solicitaste restablecer tu contraseña, ignora este correo.</p>
                ";
            }
            else
            {
                content = content.Replace("{{RESET_CODE}}", resetCode);
            }

            await SendEmailAsync(to, subject, content);
        }

        public async Task SendWelcomeEmailAsync(string to, string name)
        {
            var subject = "¡Bienvenido a Rent2Go! - Código de Verificación";
            
            // Generar código de verificación de 6 dígitos
            var verificationCode = GenerateVerificationCode();
            
            // Guardar el código en la base de datos
            try 
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == to.ToLower());
                
                if (user != null)
                {
                    // Eliminar verificaciones previas para este usuario
                    var previousVerifications = await System.Linq.Queryable.Where(_context.EmailVerifications, v => v.UserId == user.Id && v.VerifiedAt == null)
                        .ToListAsync();
                        
                    foreach (var verification in previousVerifications)
                    {
                        _context.EmailVerifications.Remove(verification);
                    }
                    
                    // Crear nueva verificación
                    var emailVerification = new EmailVerification
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = user.Id,
                        Email = user.Email,
                        VerificationToken = verificationCode,
                        ExpiresAt = DateTime.UtcNow.AddHours(1),
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.EmailVerifications.Add(emailVerification);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine("Código de verificación guardado para el usuario {0}: {1}", user.Email, verificationCode);
                }
                else
                {
                    Console.WriteLine($"Usuario no encontrado para email: {to}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar código de verificación: {ex.Message}");
            }
            
            // Enviar email de bienvenida con código de verificación
            var template = GetEmailTemplate("welcome.html");
            
            if (string.IsNullOrEmpty(template))
            {
                // Si no encuentra la plantilla, usa una versión básica
                template = $@"
                    <h1>¡Bienvenido a Rent2Go, {name}!</h1>
                    <p>Gracias por registrarte en nuestra plataforma. Para verificar tu cuenta, usa el siguiente código:</p>
                    <h2 style='color: #4CAF50; font-size: 24px;'>{verificationCode}</h2>
                    <p>Este código expirará en 1 hora.</p>
                ";
            }
            else
            {
                template = template.Replace("{{NAME}}", name);
                template = template.Replace("{{VERIFICATION_CODE}}", verificationCode);
            }
            
            await SendEmailAsync(to, subject, template);
        }

        public async Task SendVerificationEmailAsync(string to, string name, string verificationToken)
        {
            var subject = "Verifica tu correo - Rent2Go";
            var template = GetEmailTemplate("verification_email.html");
            
            if (string.IsNullOrEmpty(template))
            {
                // Si no encuentra la plantilla, usa una versión básica
                template = $@"
                    <h1>Verifica tu correo electrónico</h1>
                    <p>Hola {name},</p>
                    <p>Gracias por registrarte en Rent2Go. Para verificar tu dirección de correo electrónico, haz clic en el siguiente enlace:</p>
                    <p><a href='https://rent2go.com/verify-email?token={verificationToken}'>Verificar mi correo</a></p>
                    <p>O usa el siguiente código en la aplicación:</p>
                    <h2 style='color: #4CAF50; font-size: 24px;'>{verificationToken}</h2>
                    <p>Este enlace expirará en 24 horas.</p>
                ";
            }
            else
            {
                template = template.Replace("{{NAME}}", name);
                template = template.Replace("{{VERIFICATION_TOKEN}}", verificationToken);
            }
            
            await SendEmailAsync(to, subject, template);
        }

        public async Task SendVerificationCodeEmailAsync(string to, string name, string verificationCode)
        {
            var subject = "Código de Verificación - Rent2Go";
            var template = GetEmailTemplate("verification_code.html");
            
            if (string.IsNullOrEmpty(template))
            {
                // Si no encuentra la plantilla, usa una versión básica
                template = $@"
                    <h1>Código de Verificación</h1>
                    <p>Hola {name},</p>
                    <p>Usa el siguiente código para verificar tu cuenta:</p>
                    <h2 style='color: #4CAF50; font-size: 24px;'>{verificationCode}</h2>
                    <p>Este código expirará en 1 hora.</p>
                ";
            }
            else
            {
                template = template.Replace("{{NAME}}", name);
                template = template.Replace("{{VERIFICATION_CODE}}", verificationCode);
            }
            
            await SendEmailAsync(to, subject, template);
        }
        
        private string GetEmailTemplate(string templateName)
        {
            try
            {
                var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
                var templatePath = Path.Combine(basePath, templateName);
                
                if (File.Exists(templatePath))
                {
                    return File.ReadAllText(templatePath);
                }
                
                // Si no lo encuentra en el directorio base, intenta con la ruta del proyecto
                var projectPath = Directory.GetCurrentDirectory();
                templatePath = Path.Combine(projectPath, "EmailTemplates", templateName);
                
                if (File.Exists(templatePath))
                {
                    return File.ReadAllText(templatePath);
                }
                
                Console.WriteLine($"No se encontró la plantilla de correo: {templateName}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer plantilla de correo {templateName}: {ex.Message}");
                return string.Empty;
            }
        }
        
        private string GenerateVerificationCode()
        {
            // Generar código numérico de 6 dígitos
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
