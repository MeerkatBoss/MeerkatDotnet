using System.Runtime.Serialization;

namespace MeerkatDotnet.Services.Database.Exceptions;

public class TokenExpiredException : System.Exception
{
    public TokenExpiredException() { }
    public TokenExpiredException(string message) : base(message) { }
    public TokenExpiredException(string message, Exception inner) : base(message, inner) { }
    public TokenExpiredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}