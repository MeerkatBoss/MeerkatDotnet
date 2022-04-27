namespace MeerkatDotnet.Models.Responses;

public record LogInResponse(
    string RefreshToken,
    string AccessToken,
    UserOutputModel User
);