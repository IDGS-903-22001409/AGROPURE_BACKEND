using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AGROPURE.Data;
using AGROPURE.Helpers;
using AGROPURE.Middleware;
using AGROPURE.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AGROPURE API",
        Version = "v1",
        Description = "API para Sistema de Monitoreo de Agua IoT",
        Contact = new OpenApiContact
        {
            Name = "AGROPURE Team",
            Email = "info@agropure.com"
        }
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                }
            },
            new string[] {}
        }
    });
});

// Database
builder.Services.AddDbContext<AgroContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services - registrar todos los servicios
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<QuoteService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<SaleService>();
builder.Services.AddScoped<CostingService>();
builder.Services.AddScoped<EmailService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// JWT Authentication
var jwtKey = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Secret key is not configured");
}

var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true
    };
});

// CORS - configuración más permisiva para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AGROPURE API v1");
        c.RoutePrefix = "swagger";
    });
}

// Middlewares en orden correcto
app.UseHttpsRedirection();

// CORS antes de autenticación
app.UseCors("AllowAngular");

// Middleware personalizado
app.UseMiddleware<ErrorHandlingMiddleware>();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

// Initialize database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AgroContext>();

        // Asegurar que la base de datos existe
        context.Database.EnsureCreated();

        // Aplicar migraciones pendientes si las hay
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }

        // Inicializar datos
        DbInitializer.Initialize(context);

        Console.WriteLine("Database initialized successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database");
        throw;
    }
}

Console.WriteLine($"AGROPURE API is running on {app.Environment.EnvironmentName} environment");
Console.WriteLine($"Swagger UI available at: /swagger");

app.Run();