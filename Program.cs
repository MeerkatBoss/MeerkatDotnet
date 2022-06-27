using MeerkatDotnet.Database;
using MeerkatDotnet.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MeerkatDotnet.Repositories;
using MeerkatDotnet.Services;
using MeerkatDotnet.Endpoints;
using Microsoft.IdentityModel.Tokens;
using MeerkatDotnet.Middleware;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
config.AddEnvironmentVariables();

// Load options from config
JwtOptions jwtOptions = JwtOptions.FromConfiguration(config);
HashingOptions hashingOptions = HashingOptions.FromConfiguration(config);

builder.Services.AddSingleton<JwtOptions>(jwtOptions);
builder.Services.AddSingleton<HashingOptions>(hashingOptions);

// Configure database context
string db_address = config.GetValue<string>("DB_ADDRESS");
string db_port = config.GetValue<string>("DB_PORT");
string db_name = config.GetValue<string>("DB_NAME");
string db_user = config.GetValue<string>("DB_USER");
string db_password = config.GetValue<string>("DB_PASSWORD");
string connectionString =
    $"Server={db_address};Port={db_port};Database={db_name};User Id={db_user};Password={db_password}";
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connectionString));

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
        c => c.SwaggerDoc("v2", new () { Title = "MeerkatDotnet", Version = "v2"}));

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

app.MapUsersEndpoints("/api/v2", "user");
app.UseMiddleware<ValidationExceptionMiddleware>();
app.UseMiddleware<NotFoundMiddleware>();
app.UseMiddleware<LoginFailedMiddleware>();

// Use Swagger if in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "MeerkatDotnet v2"));
}

await app.RunAsync();
