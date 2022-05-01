using System.Runtime.Serialization;

namespace MeerkatDotnet.Repositories.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException() { }
    public UserNotFoundException(string message) : base(message) { }
    public UserNotFoundException(string message, Exception inner) : base(message, inner) { }
    public UserNotFoundException(
        SerializationInfo info, StreamingContext context
        ) : base(info, context) { }
}