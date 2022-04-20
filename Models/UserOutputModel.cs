namespace MeerkatDotnet.Models;

public record UserOutputModel(
    int Id,
    string Username,
    string? Email,
    string? Phone
);