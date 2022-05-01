using System.Runtime.Serialization;

namespace MeerkatDotnet.Repositories.Exceptions;

public class UsernameTakenException : System.Exception
{
    public UsernameTakenException() { }
    public UsernameTakenException(string message) : base(message) { }
    public UsernameTakenException(string message, Exception inner)
        : base(message, inner) { }
    public UsernameTakenException(
        SerializationInfo info,
        StreamingContext context)
        : base(info, context) { }
}