using System.ComponentModel.DataAnnotations;
namespace MeerkatDotnet.Models;

public record UserInputModel(
    [Required] string Username,
    [Required] string Password,
    [EmailAddress] string? Email,
    [Phone] string? Phone
);