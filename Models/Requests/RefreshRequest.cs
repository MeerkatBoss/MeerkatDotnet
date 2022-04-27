namespace MeerkatDotnet.Models.Requests;

public record RefreshRequest(
    string RefreshToken,
    string AccessToken
);