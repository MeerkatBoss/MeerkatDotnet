using MeerkatDotnet.Database;
using MeerkatDotnet.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MeerkatDotnet.Repositories;
using MeerkatDotnet.Services;
using MeerkatDotnet.Endpoints;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Load JWT options from config
JwtOptions jwtOptions = new();
HashingOptions hashingOptions = new();
//builder.Services.Configure<JwtOptions>(config);
config.Bind("JwtOptions", jwtOptions);
config.Bind("HashingOptions", hashingOptions);

builder.Services.AddSingleton<JwtOptions>(jwtOptions);
builder.Services.AddSingleton<HashingOptions>(hashingOptions);

// Load hashing options from config
//builder.Services.Configure<HashingOptions>(config);

// Configure database context
var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString));

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure authorization and authentication via JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new()
        {
            // Issuer validation: on
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            // Audience validation: on
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            // Lifetime validation: on
            ValidateLifetime = true,

            // Issuer signing key validation: on
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwtOptions.SecurityKey
        };
        options.Events = new()
        {
            OnAuthenticationFailed = (context) => {
                if(context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers["X-Token-Expired"] = "true";
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddScoped<IRepositoryContext, RepositoryContext>();
builder.Services.AddScoped<IUsersService, UsersService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapUsersEndpoints("/api/v1", "user");

// Use Swagger if in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();
