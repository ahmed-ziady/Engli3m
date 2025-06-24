using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure;
using Engli3m.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add DbContext
builder.Services.AddDbContext<EnglishDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<User, Role>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<EnglishDbContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
    options.AddPolicy("Teacher", policy => policy.RequireRole("Teacher"));
    options.AddPolicy("Assistant", policy => policy.RequireRole("Assistant"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Teacher", "Assistant"));
});

// Register Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthServices, AuthServices>();

// Configure Controllers
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Engli3m API",
        Version = "v1",
        Description = "API documentation for the Engli3m educational platform"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var userManager = services.GetRequiredService<UserManager<User>>();

        await SeedRolesAndUsers(roleManager, userManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// 1. Enable Routing
app.UseRouting();

// 2. HTTPS and CORS
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 3. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 4. Map Controllers
app.MapControllers();

// 5. Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// 6. Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Engli3m API V1"));

app.Run();

// Seeding Function
static async Task SeedRolesAndUsers(RoleManager<Role> roleManager, UserManager<User> userManager)
{
    // Seed Roles
    string[] roleNames = { "Student", "Teacher", "Assistant" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new Role(roleName));
        }
    }

    // Seed Teacher Account
    var teacherEmail = "momenhelmy085@gmail.com";
    var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
    if (teacherUser == null)
    {
        teacherUser = new User
        {
            UserName = teacherEmail,
            Email = teacherEmail,
            FirstName = "System",
            LastName = "Teacher",
            Grade = "N/A",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(teacherUser, "momenPass1234!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(teacherUser, "Teacher");
        }
    }

    // Seed Assistant Account
    var assistantEmail = "mariam3012004maroo@gmail.com";
    var assistantUser = await userManager.FindByEmailAsync(assistantEmail);
    if (assistantUser == null)
    {
        assistantUser = new User
        {
            UserName = assistantEmail,
            Email = assistantEmail,
            FirstName = "System",
            LastName = "Assistant",
            Grade = "N/A",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(assistantUser, "Assistmariam#1");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(assistantUser, "Assistant");
        }
    }
}
