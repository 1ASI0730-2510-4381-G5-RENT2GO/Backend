using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BackendRent2Go.Data;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using System.IdentityModel.Tokens.Jwt;
using Rent2Go.API.Shared.Domain.Services;
using BackendRent2Go.Shared.Infrastructure.Services;
using Rent2Go.API.IAM.Domain.Repositories;
using Rent2Go.API.IAM.Infrastructure.Persistence.EFC.Repositories;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// En desarrollo, usar automáticamente appsettings.Local.json con todas las claves reales
if (builder.Environment.IsDevelopment())
{
    var localSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Local.json");
    if (File.Exists(localSettingsPath))
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: false, reloadOnChange: true);
        Console.WriteLine("✅ Configuración local cargada (appsettings.Local.json)");
    }
    else
    {
        Console.WriteLine("⚠️ Archivo appsettings.Local.json no encontrado - usando variables de entorno");
    }
}

// Obtener variables de configuración (ahora desde appsettings.Local.json en desarrollo)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
    throw new InvalidOperationException("La cadena de conexión no está configurada");
    
string tokenKey = builder.Configuration["TokenKey"] ?? 
    Environment.GetEnvironmentVariable("TOKEN_KEY") ?? 
    throw new InvalidOperationException("El TokenKey no está configurado");

// Configurar SendGrid
string sendGridApiKey = builder.Configuration["SendGridSettings:ApiKey"] ?? 
    Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? 
    throw new InvalidOperationException("La API Key de SendGrid no está configurada");

Console.WriteLine($"✅ Conexión configurada: {connectionString.Substring(0, Math.Min(30, connectionString.Length))}...");
Console.WriteLine($"✅ Token configurado: {!string.IsNullOrEmpty(tokenKey)}");
Console.WriteLine($"✅ SendGrid configurado: {!string.IsNullOrEmpty(sendGridApiKey)}");

// Configurar servicios
// Servicio de email (ahora en la capa Shared)
builder.Services.AddScoped<Rent2Go.API.Shared.Domain.Services.IEmailService, EmailService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configurar el serializador para ignorar referencias circulares
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Registrar servicios de IAM
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();

// Registrar servicios de Provider (Vehículos)
builder.Services.AddScoped<Rent2Go.API.Provider.Domain.Repositories.IVehicleRepository, Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories.VehicleRepository>();
builder.Services.AddScoped<Rent2Go.API.Provider.Domain.Repositories.IVehicleSpecificationRepository, Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories.VehicleSpecificationRepository>();
builder.Services.AddScoped<Rent2Go.API.Provider.Domain.Repositories.IVehicleImageRepository, Rent2Go.API.Provider.Infrastructure.Persistence.EFC.Repositories.EFCVehicleImageRepository>();
builder.Services.AddScoped<Rent2Go.API.Provider.Domain.Services.IVehicleService, Rent2Go.API.Provider.Domain.Services.VehicleService>();

// Registrar interfaces ACL del Provider (necesarias para las clases ACL del Client)
builder.Services.AddScoped<Rent2Go.API.Provider.Interfaces.ACL.IProviderVehicleService, Rent2Go.API.Provider.Application.ACL.ProviderVehicleServiceACL>();
builder.Services.AddScoped<Rent2Go.API.Provider.Interfaces.ACL.IProviderVehicleImageService, Rent2Go.API.Provider.Application.ACL.ProviderVehicleImageServiceACL>();

// Registrar servicios del módulo Client (Vehículos para clientes)
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Services.IVehicleService, BackendRent2Go.Client.Application.Internal.VehicleService>();
builder.Services.AddScoped<BackendRent2Go.Client.Interfaces.ACL.IProviderVehicleService, BackendRent2Go.Client.Application.ACL.ProviderVehicleServiceAcl>();
builder.Services.AddScoped<BackendRent2Go.Client.Interfaces.ACL.IProviderVehicleImageService, BackendRent2Go.Client.Application.ACL.ProviderVehicleImageServiceAcl>();

// Registrar servicios del módulo Client (Reservas para clientes)
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Services.IReservationService, BackendRent2Go.Client.Application.Internal.CommandServices.ReservationService>();

// Registrar servicios del módulo Provider (Reservas para providers)
builder.Services.AddScoped<BackendRent2Go.Provider.Domain.Services.IProviderReservationService, BackendRent2Go.Provider.Application.Internal.CommandServices.ProviderReservationService>();

// Registrar repositorios de pagos
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Repositories.IPaymentMethodRepository, BackendRent2Go.Client.Infrastructure.Persistence.EFC.Repositories.PaymentMethodRepository>();
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Repositories.IPaymentRepository, BackendRent2Go.Client.Infrastructure.Persistence.EFC.Repositories.PaymentRepository>();

// Registrar servicios de pagos con las nuevas implementaciones
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Services.IPaymentMethodService, BackendRent2Go.Client.Application.Internal.PaymentMethodService>();
builder.Services.AddScoped<BackendRent2Go.Client.Domain.Services.IPaymentService, BackendRent2Go.Client.Application.Internal.PaymentService>();

// Registrar UnitOfWork
builder.Services.AddScoped<BackendRent2Go.Shared.Domain.Repositories.IUnitOfWork, BackendRent2Go.Shared.Infrastructure.Persistence.EFC.Repositories.UnitOfWork>();

builder.Services.AddScoped<BackendRent2Go.Shared.Domain.Services.IDateTimeProvider, BackendRent2Go.Shared.Infrastructure.Services.DateTimeProvider>();

// Configurar DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseSnakeCaseNamingConvention();
});

// Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true,
        RequireExpirationTime = true
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Rutas públicas que no requieren autenticación
            string path = context.HttpContext.Request.Path;
            if (path.ToString().StartsWith("/api/auth/login") || 
                path.ToString().StartsWith("/api/auth/register") || 
                path.ToString().StartsWith("/swagger") ||
                path.ToString().StartsWith("/api/auth/forgot-password") ||
                path.ToString().StartsWith("/api/auth/reset-password") ||
                path.ToString().StartsWith("/api/auth/verify-email") ||
                path.ToString().StartsWith("/images"))
            {
                // No intentar extraer token para rutas públicas
                return Task.CompletedTask;
            }
            
            var accessToken = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer "))
            {
                context.Token = accessToken.Substring("Bearer ".Length).Trim();
                Console.WriteLine($"JWT Token extraído: '{context.Token}'");
                
                // Verificar formato del token para depuración
                var parts = context.Token.Split('.');
                if (parts.Length != 3)
                {
                    Console.WriteLine($"Token formato incorrecto. Partes: {parts.Length}");
                    if (parts.Length > 0)
                    {
                        foreach (var part in parts)
                        {
                            Console.WriteLine($"Parte del token: '{part}'");
                        }
                    }
                }
            }
            else
            {
                // Solo mostrar este mensaje para rutas protegidas (no públicas)
                Console.WriteLine("No se proporcionó token de autorización o no tiene el formato esperado");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var jwtToken = context.SecurityToken as JwtSecurityToken;
            var userId = jwtToken?.Claims.FirstOrDefault(c => c.Type == "nameid" || c.Type == "sub")?.Value;
            Console.WriteLine($"Token validado exitosamente para usuario ID: {userId}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Auth failed: {context.Exception.GetType().Name}: {context.Exception.Message}");
            
            // Identificar tipo específico de error para mejorar depuración
            if (context.Exception is SecurityTokenExpiredException)
            {
                Console.WriteLine("El token ha expirado");
                context.Response.Headers.Add("Token-Expired", "true");
            }
            else if (context.Exception is SecurityTokenValidationException)
            {
                Console.WriteLine("El token no es válido");
            }
            
            if (context.Exception.InnerException != null)
                Console.WriteLine($"Inner exception: {context.Exception.InnerException.Message}");
                
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Añadir más información al error para depuración
            if (string.IsNullOrEmpty(context.Error))
            {
                context.Error = "invalid_token";
                // Usar solo caracteres ASCII en los mensajes de error que se pasan a los headers
                context.ErrorDescription = "Token ausente, invalido o expirado";
            }
            
            Console.WriteLine($"JWT Challenge error: {context.Error}, Description: {context.ErrorDescription}");
            Console.WriteLine($"Authentication scheme: {context.AuthenticateFailure?.Message ?? "No authenticate failure"}");
            
            // Prevenir que el error se propague a los headers HTTP
            context.HandleResponse();
            
            // Escribir una respuesta personalizada sin caracteres no-ASCII
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            var response = new { error = "unauthorized", message = "No autorizado" };
            return context.Response.WriteAsJsonAsync(response);
        }
    };
});

// Configurar autenticación externa solo si las credenciales están disponibles
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientId");
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? Environment.GetEnvironmentVariable("Authentication__Google__ClientSecret");

var facebookAppId = builder.Configuration["Authentication:Facebook:AppId"] ?? Environment.GetEnvironmentVariable("Authentication__Facebook__AppId");
var facebookAppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? Environment.GetEnvironmentVariable("Authentication__Facebook__AppSecret");

// Solo agregar Google si las credenciales están configuradas
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });
    Console.WriteLine("Autenticación con Google configurada");
}
else
{
    Console.WriteLine("Autenticación con Google omitida (credenciales no configuradas)");
}

// Solo agregar Facebook si las credenciales están configuradas
if (!string.IsNullOrEmpty(facebookAppId) && !string.IsNullOrEmpty(facebookAppSecret))
{
    builder.Services.AddAuthentication().AddFacebook(options =>
    {
        options.AppId = facebookAppId;
        options.AppSecret = facebookAppSecret;
    });
    Console.WriteLine("Autenticación con Facebook configurada");
}
else
{
    Console.WriteLine("Autenticación con Facebook omitida (credenciales no configuradas)");
}

// Configurar CORS para desarrollo y producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://rent2go-g5.netlify.app";
        var localFrontendUrl = Environment.GetEnvironmentVariable("LOCAL_FRONTEND_URL") ?? "http://localhost:5173";
        
        policy.WithOrigins(frontendUrl, localFrontendUrl, "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configurar Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Rent2Go API",
        Version = "v1",
        Description = "API RESTful para la aplicación de alquiler de vehículos Rent2Go",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo",
            Email = "contact@rent2go.com",
            Url = new Uri("https://rent2go.com")
        }
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// Habilitar Swagger tanto en desarrollo como en producción para facilitar las pruebas
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rent2Go API v1");
    c.RoutePrefix = string.Empty;
});

// app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

// Habilitar archivos estáticos (para servir imágenes de perfil pendrive en wwwroot)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Migrar la base de datos automáticamente
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var retry = 0;
        const int maxRetries = 5;
        while (retry < maxRetries)
        {
            try
            {
                context.Database.EnsureCreated();
                Console.WriteLine("Base de datos creada o existente exitosamente.");
                break;
            }
            catch (Exception ex)
            {
                retry++;
                if (retry >= maxRetries)
                {
                    Console.WriteLine($"Error crítico al conectar con la base de datos después de {maxRetries} intentos: {ex.Message}");
                    if (ex.InnerException != null)
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    throw;
                }
                else
                {
                    Console.WriteLine($"Intento {retry} fallido al conectar con la base de datos: {ex.Message}");
                    await Task.Delay(2000 * retry); // Esperar más tiempo con cada reintento
                }
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la inicialización de la aplicación");
    }
}

app.Run();
