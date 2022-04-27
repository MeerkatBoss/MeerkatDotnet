using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MeerkatDotnet.Models;

namespace MeerkatDotnet.Database.Models;


[Table("users")]
public class UserModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [MinLength(3)]
    [MaxLength(20)]
    [Required]
    [Column("username")]
    public string Username { get; set; }

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [EmailAddress]
    [Column("email")]
    public string? Email { get; set; }

    [Phone]
    [Column("phone")]
    public string? Phone { get; set; }

    // [Column("refresh_tokens")]
    // public List<RefreshTokenModel>? RefreshTokens { get; set; }

    public UserModel(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    public UserModel(string username, string passwordHash, string? email, string? phone)
    {
        Username = username;
        PasswordHash = passwordHash;
        Email = email;
        Phone = phone;
    }

    public UserModel Clone()
    {
        return new UserModel(
            username: Username,
            passwordHash: PasswordHash,
            email: Email,
            phone: Phone)
        {
            Id = this.Id
        };
    }

    public static implicit operator UserOutputModel(UserModel user)
    {
        return new(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            Phone: user.Phone
        );
    }

}