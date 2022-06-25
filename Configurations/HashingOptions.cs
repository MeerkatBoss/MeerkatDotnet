using System.Text;

namespace MeerkatDotnet.Configurations;

public sealed class HashingOptions
{
    public string Salt { get; set; } = null!;
    public int IterationCount { get; set; }

    public byte[] SaltBytes
    {
        get => Encoding.UTF8.GetBytes(Salt);
    }

    public static HashingOptions FromConfiguration(ConfigurationManager config)
    {
        return new()
        {
            Salt = config.GetValue<string>("HASH_SALT"),
            IterationCount = config.GetValue<int>("HASH_ITERATIONS")
        };
    }
}
