using Engli3m.Application.Interfaces;
using Engli3m.Domain.Enities;
using Engli3m.Infrastructure;
using Engli3m.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
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

builder.Services.AddAuthorizationBuilder()
  .AddPolicy("Student", policy => policy.RequireRole("Student"))
  .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 3221225472; // 3 GB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 3221225472;

});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 3221225472; // 3 GB
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromHours(3);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

// 5. Register Application Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IAdminService, AdminServices>();
builder.Services.AddScoped<IStudentService, StudentServices>();
builder.Services.AddScoped<IProfile, ProfileService>();
builder.Services.AddScoped<IPostServices, PostServices>();
builder.Services.AddScoped<IEQuizServices, EQuizServices>();
builder.Services.AddHostedService<MonthlyScoreResetService>();
builder.Services.AddHostedService<PaymentCheckService>();
//builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, FirebaseNotificationService>();

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
    const string teacherEmail8 = "admin8@gmail.com";
    var normalizedTeacherEmail8 = teacherEmail8.ToUpper();
    var teacherUser8 = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedTeacherEmail8);

    if (teacherUser8 == null)
    {
        teacherUser8 = new User
        {
            UserName = teacherEmail8,
            Email = teacherEmail8,
            FirstName = "Mr. Mo'men",
            LastName = "Helmy",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null,
            CleanPassword = "adminPass1234!"
        };
        var result = await userManager.CreateAsync(teacherUser8, "adminPass1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(teacherUser8, "Admin");
    }
    const string teacherEmail7 = "admin7@gmail.com";
    var normalizedTeacherEmail7 = teacherEmail7.ToUpper();
    var teacherUser7 = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedTeacherEmail7);

    if (teacherUser7 == null)
    {
        teacherUser7 = new User
        {
            UserName = teacherEmail7,
            Email = teacherEmail7,
            FirstName = "Mr. Mo'men",
            LastName = "Helmy",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null
        };
        var result = await userManager.CreateAsync(teacherUser7, "adminPass1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(teacherUser7, "Admin");
    }
    const string teacherEmail6 = "admin6@gmail.com";
    var normalizedTeacherEmail6 = teacherEmail6.ToUpper();
    var teacherUser6 = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedTeacherEmail6);

    if (teacherUser6 == null)
    {
        teacherUser6 = new User
        {
            UserName = teacherEmail6,
            Email = teacherEmail6,
            FirstName = "Mr. Mo'men",
            LastName = "Helmy",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null
        };
        var result = await userManager.CreateAsync(teacherUser6, "adminPass1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(teacherUser6, "Admin");
    }
    const string teacherEmail5 = "admin5@gmail.com";
    var normalizedTeacherEmail5 = teacherEmail5.ToUpper();
    var teacherUser5 = await userManager.Users
        .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedTeacherEmail5);

    if (teacherUser5 == null)
    {
        teacherUser5 = new User
        {
            UserName = teacherEmail5,
            Email = teacherEmail5,
            FirstName = "Mr. Mo'men",
            LastName = "Helmy",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            CurrentJwtToken = null
        };
        var result = await userManager.CreateAsync(teacherUser5, "adminPass1234!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(teacherUser5, "Admin");
    }



}