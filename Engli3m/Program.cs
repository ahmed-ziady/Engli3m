using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure;
using Engli3m.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. Add DbContext
builder.Services.AddDbContext<EnglishDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

// 2. Configure Identity
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

// 3. Configure JWT Authentication
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
    options.Events= new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
            var userId = context.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                context.Fail("Invalid token: missing user ID.");
                return;
            }
            var user = await userManager.FindByIdAsync(userId);

            if (user == null || user.IsLocked)
            {
                context.Fail("User not found or account is locked.");
                return;
            }
        }
    };

}
);

// 4. Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Student", policy => policy.RequireRole("Student"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// 5. Register Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IAdminService, AdminServices>();
builder.Services.AddScoped<IStudentService, StudentServices>();
// 6. Add Controllers
builder.Services.AddControllers();

// 7. Configure Swagger/OpenAPI
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

// 8. Configure CORS
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

// 9. Seed Roles & Users
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Fix NULL tokens before seeding
        var db = services.GetRequiredService<EnglishDbContext>();
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE AspNetUsers SET CurrentJwtToken = '' WHERE CurrentJwtToken IS NULL");

        var roleManager = services.GetRequiredService<RoleManager<Role>>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        await SeedRolesAndUsers(roleManager, userManager);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Seeding error");
    }
}

app.UseCors("AllowAll");

// 3. Static Files עם כותרות CORS
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
    RequestPath = "",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, OPTIONS");
        ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type");
    }
});

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Engli3m API V1"));

app.Run();

// Helper: Seed Roles & System Users
static async Task SeedRolesAndUsers(RoleManager<Role> roleManager, UserManager<User> userManager)
{
    string[] roleNames = ["Student", "Admin"];

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new Role(roleName));
    }

    // System Teacher (Admin role)
    const string teacherEmail = "momenhelmy085@gmail.com";
    var normalizedTeacherEmail = teacherEmail.ToUpper();
    var teacherUser = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedTeacherEmail);

    if (teacherUser == null)
    {
        teacherUser = new User
        {
            UserName = teacherEmail,
            Email = teacherEmail,
            FirstName = "System",
            LastName = "Teacher",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null
        };
        var result = await userManager.CreateAsync(teacherUser, "momenPass1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(teacherUser, "Admin");
    }

    // System Assistant (Admin role)
    const string assistantEmail = "mariam3012004maroo@gmail.com";
    var normalizedAssistantEmail = assistantEmail.ToUpper();
    var assistantUser = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedAssistantEmail);

    if (assistantUser == null)
    {
        assistantUser = new User
        {
            UserName = assistantEmail,
            Email = assistantEmail,
            FirstName = "System",
            LastName = "Assistant",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null
        };
        var result = await userManager.CreateAsync(assistantUser, "Assistmariam#1");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(assistantUser, "Admin");
    }
}