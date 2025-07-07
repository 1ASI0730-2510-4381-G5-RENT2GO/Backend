using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Threading.Tasks;
using System.IO;
using SendGrid;
using SendGrid.Helpers.Mail;
using BackendRent2Go.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Rent2Go.API.IAM.Domain.Model;
using Rent2Go.API.IAM.Domain.Model.Aggregates;
using Rent2Go.API.Shared.Domain.Services;
using System.Net;

namespace Rent2Go.API.IAM.Domain.Services
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
                    var previousVerifications = await _context.EmailVerifications
                        .Where(v => v.UserId == user.Id && v.VerifiedAt == null)
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
                    
                    Console.WriteLine($"Código de verificación guardado para el usuario {user.Email}: {verificationCode}");
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
            
            // Codificar el email para la URL
            var encodedEmail = WebUtility.UrlEncode(to);
            
            // Obtener la plantilla de correo combinada con verificación
            var content = GetEmailTemplate("verification_code.html");
            
            if (string.IsNullOrEmpty(content))
            {
                // Si no encuentra la plantilla, usa una versión básica con verificación incluida
                content = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Bienvenido a Rent2Go - Verificación de Cuenta</title>
                        <style>
                            body {{
                                font-family: 'Segoe UI', Arial, sans-serif;
                                line-height: 1.6;
                                color: #333;
                                margin: 0;
                                padding: 0;
                                background-color: #f9f9f9;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                padding: 0;
                                background-color: #ffffff;
                                border-radius: 16px;
                                box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
                                overflow: hidden;
                            }}
                            .header {{
                                text-align: center;
                                padding: 30px 0;
                                background: linear-gradient(135deg, #6366F1 0%, #4F46E5 100%);
                                color: white;
                            }}
                            .code {{
                                font-size: 36px;
                                font-weight: bold;
                                text-align: center;
                                color: #4F46E5;
                                letter-spacing: 8px;
                                margin: 30px 0;
                                padding: 15px;
                                background-color: #f5f5f5;
                                border-radius: 12px;
                            }}
                            .content {{
                                padding: 40px 30px;
                                text-align: center;
                            }}
                            .button {{
                                display: inline-block;
                                background-color: #4F46E5;
                                color: white !important;
                                font-weight: bold;
                                text-decoration: none;
                                padding: 14px 30px;
                                border-radius: 10px;
                                margin: 30px 0 20px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>¡Bienvenido a Rent2Go!</h1>
                            </div>
                            <div class='content'>
                                <p>Hola <strong>{name}</strong>,</p>
                                <p>Gracias por registrarte en Rent2Go, tu plataforma de alquiler de vehículos.</p>
                                <p>Para verificar tu cuenta y comenzar a utilizar nuestros servicios, ingresa el siguiente código en la página de verificación:</p>
                                <div class='code'>{verificationCode}</div>
                                <p>Este código expirará en 60 minutos.</p>
                                <a href='http://localhost:5173/verify-code?email={encodedEmail}' class='button'>Ir a la página de verificación</a>
                                <p>Si no has solicitado este código, por favor ignora este correo.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";
            }
            else
            {
                content = content
                    .Replace("{{NAME}}", name)
                    .Replace("{{VERIFICATION_CODE}}", verificationCode)
                    .Replace("{{EMAIL_ENCODED}}", encodedEmail);
            }

            await SendEmailAsync(to, subject, content);
            
            // Ya no enviamos un segundo correo de verificación
            Console.WriteLine($"Correo de bienvenida con código de verificación enviado a {to}");
        }

        public async Task SendVerificationEmailAsync(string to, string name, string verificationToken)
        {
            var subject = "Verificación de email - Rent2Go";
            var verificationLink = $"{_config["AppBaseUrl"] ?? "http://localhost:3000"}/verificar-email?token={verificationToken}";
            var content = GetEmailTemplate("verification_email.html");
            
            if (string.IsNullOrEmpty(content))
            {
                // Si no encuentra la plantilla, usa una versión básica
                content = $@"
                    <h1>Verificación de email</h1>
                    <p>Hola {name},</p>
                    <p>Gracias por registrarte en Rent2Go. Para completar tu registro y verificar tu email, haz clic en el siguiente botón:</p>
                    <div style='text-align: center; margin: 20px 0;'>
                        <a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; font-weight: bold;'>
                            Verificar mi email
                        </a>
                    </div>
                    <p>O copia y pega el siguiente enlace en tu navegador:</p>
                    <p style='color: #0066cc;'>{verificationLink}</p>
                    <p>Este enlace expirará en 3 días.</p>
                    <p>Si no has solicitado esta verificación, por favor ignora este correo.</p>
                ";
            }
            else
            {
                content = content
                    .Replace("{{NAME}}", name)
                    .Replace("{{VERIFICATION_LINK}}", verificationLink);
            }

            await SendEmailAsync(to, subject, content);
        }
        
        // Implementación del método público para enviar códigos de verificación
        public async Task SendVerificationCodeEmailAsync(string to, string name, string verificationCode)
        {
            var subject = "Código de Verificación - Rent2Go";
            
            // Obtener la plantilla de correo
            var content = GetEmailTemplate("verification_code.html");
            
            // Codificar el email para la URL
            var encodedEmail = WebUtility.UrlEncode(to);
            
            if (string.IsNullOrEmpty(content))
            {
                // Si no encuentra la plantilla, usa una versión básica
                content = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>Verificación de Cuenta - Rent2Go</title>
                        <style>
                            body {{
                                font-family: 'Segoe UI', Arial, sans-serif;
                                line-height: 1.6;
                                color: #333;
                                margin: 0;
                                padding: 0;
                                background-color: #f9f9f9;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 20px auto;
                                padding: 0;
                                background-color: #ffffff;
                                border-radius: 16px;
                                box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
                                overflow: hidden;
                            }}
                            .header {{
                                text-align: center;
                                padding: 30px 0;
                                background: linear-gradient(135deg, #6366F1 0%, #4F46E5 100%);
                                color: white;
                            }}
                            .code {{
                                font-size: 36px;
                                font-weight: bold;
                                text-align: center;
                                padding: 20px;
                                margin: 20px auto;
                                background-color: #f1f5f9;
                                border-radius: 8px;
                                max-width: 200px;
                                letter-spacing: 2px;
                                color: #4F46E5;
                            }}
                            .content {{
                                padding: 30px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>Verificación de Email</h1>
                            </div>
                            <div class='content'>
                                <p>Hola {name},</p>
                                <p>Gracias por registrarte en Rent2Go. Para verificar tu cuenta, por favor utiliza el siguiente código:</p>
                                
                                <div class='code'>{verificationCode}</div>
                                
                                <p>Este código expirará en 1 hora. Si no has solicitado esta verificación, por favor ignora este mensaje.</p>
                                
                                <p>Saludos,<br>El Equipo de Rent2Go</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";
            }
            else
            {
                // Reemplazar variables en la plantilla
                content = content
                    .Replace("{{NAME}}", name)
                    .Replace("{{VERIFICATION_CODE}}", verificationCode)
                    .Replace("{{EMAIL_ENCODED}}", encodedEmail);
            }

            await SendEmailAsync(to, subject, content);
        }
        
        // Método público para reenviar el código de verificación
        public async Task ResendVerificationCodeAsync(string to, string name)
        {
            var verificationCode = GenerateVerificationCode();
            // Guardar el código en la base de datos o caché (implementar esta parte)
            
            await SendVerificationCodeEmailAsync(to, name, verificationCode);
        }
        
        // Método para generar un código de verificación de 6 dígitos
        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
        
        // Método para cargar plantillas HTML de correo
        private string GetEmailTemplate(string templateName)
        {
            try
            {
                // Intentar desde el directorio actual primero
                string currentDir = Directory.GetCurrentDirectory();
                string[] possiblePaths = {
                    Path.Combine(currentDir, "EmailTemplates", templateName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates", templateName)
                };
                
                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        Console.WriteLine($"Plantilla encontrada: {path}");
                        return File.ReadAllText(path);
                    }
                }
                
                Console.WriteLine($"No se encontró la plantilla: {templateName}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar plantilla {templateName}: {ex.Message}");
                return string.Empty;
            }
        }
    }
}
