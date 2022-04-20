using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MeerkatDotnet.Database.Models;

[Table("refresh_token")]
public class RefreshTokenModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("value")]
    public string Value { get; set; } = null!;

    [Required]
    [Column("user")]
    public UserModel User { get; set; } = null!;

    [Required]
    [Column("expiration_date")]
    public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsExpired
    {
        get => DateTime.UtcNow > ExpirationDate;
    }
}