namespace MeerkatDotnet.Models.Requests;

public record LogInRequest(
    string Login,
    string Password
);