using System.Text;
using Lazybook.Api.Data;
using Lazybook.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register DbContext as a scoped service in the dependency injection container
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ).UseSnakeCaseNamingConvention());
// Register custom services
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication(); // Checks if the JWT is valid

app.UseAuthorization();

app.MapControllers();

app.Run();
