namespace MeerkatDotnet.Models.Requests;

public record RefreshRequest(
    string AccessToken,
    string RefreshToken
);
