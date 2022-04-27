using MeerkatDotnet.Database;
using MeerkatDotnet.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MeerkatDotnet.Services.Database;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Load JWT options from config
JwtOptions jwtOptions = new();
builder.Services.Configure<JwtOptions>(config);
config.Bind("JwtOptions", jwtOptions);

// Load hashing options from config
builder.Services.Configure<HashingOptions>(config);

// Configure database context
var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString));

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure controllers
builder.Services.AddControllers();

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
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IQueryService, TransactionQueryService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map all controllers
app.MapControllers();

// Use Swagger if in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();