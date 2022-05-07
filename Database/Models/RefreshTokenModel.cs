using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MeerkatDotnet.Database.Models;

[Table("tokens")]
public class RefreshTokenModel
{
    // [Key]
    // [Column("id")]
    // public int Id { get; private set; }

    [Key]
    [Column("value")]
    public string Value { get; private set; } = null!;

    [Required]
    [Column("user_id")]
    public int UserId { get; private set; }

    public UserModel? User { get; private set; }

    [Required]
    [Column("expiration_date")]
    public DateTime ExpirationDate { get; private set; }

    public RefreshTokenModel(string value, int userId, DateTime expirationDate)
    {
        Value = value;
        UserId = userId;
        ExpirationDate = expirationDate;
    }

    [NotMapped]
    public bool IsExpired
    {
        get => DateTime.UtcNow > ExpirationDate;
    }

    public RefreshTokenModel Clone()
    {
        return new RefreshTokenModel(
            value: Value,
            userId: UserId,
            expirationDate: ExpirationDate
        )
        {
            // Id = this.Id,
            User = this.User?.Clone()
        };
    }

}