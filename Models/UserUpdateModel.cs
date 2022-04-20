using System.ComponentModel.DataAnnotations;
namespace MeerkatDotnet.Models;

public sealed record UserUpdateModel(
    string? Username,
    string? Password,
    [EmailAddress] string? Email,
    [Phone] string? Phone
);