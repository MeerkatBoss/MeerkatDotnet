using System.Runtime.Serialization;

namespace MeerkatDotnet.Repositories.Exceptions;

public class TokenExistsException : System.Exception
{
    public TokenExistsException() { }
    public TokenExistsException(string message) : base(message) { }
    public TokenExistsException(string message, Exception inner) : base(message, inner) { }
    public TokenExistsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}