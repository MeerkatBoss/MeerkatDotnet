namespace MeerkatDotnet.Models.Responses;

public record RefreshResponse(
    string RefreshToken,
    string AccessToken
);