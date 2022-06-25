using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MeerkatDotnet.Configurations;
public sealed class JwtOptions
{
    public string Issuer { get; set; } = default!;

    public string Audience { get; set; } = default!;

    public string Key { get; set; } = default!;

    public int AccessTokenExpirationMinutes { get; set; }

    public int RefreshTokenExpirationDays { get; set; }

    public SymmetricSecurityKey SecurityKey
    {
        get => new(Encoding.UTF8.GetBytes(Key));
    }

    public static JwtOptions FromConfiguration(ConfigurationManager config)
    {
        return new()
        {
            Issuer = config.GetValue<string>("JWT_ISSUER"),
            Audience = config.GetValue<string>("JWT_AUDIENCE"),
            Key = config.GetValue<string>("JWT_KEY"),
            AccessTokenExpirationMinutes = config.GetValue<int>("ACCESS_LIFETIME_MINUTES"),
            RefreshTokenExpirationDays = config.GetValue<int>("REFRESH_LIFETIME_DAYS")
        };
    }
}
