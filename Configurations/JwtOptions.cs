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
}