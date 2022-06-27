using System.ComponentModel.DataAnnotations;
namespace MeerkatDotnet.Models;

public sealed record UserUpdateModel(
    string OldPassword,
    string? Username = null,
    string? Password = null,
    [EmailAddress] string? Email = null,
    [Phone] string? Phone = null
);
