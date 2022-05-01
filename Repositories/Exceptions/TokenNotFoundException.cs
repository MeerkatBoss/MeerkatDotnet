using System.Runtime.Serialization;

namespace MeerkatDotnet.Repositories.Exceptions;

public class TokenNotFoundException : System.Exception
{
    public TokenNotFoundException() { }
    public TokenNotFoundException(string message) : base(message) { }
    public TokenNotFoundException(string message, Exception inner) : base(message, inner) { }
    public TokenNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}