using System.Runtime.Serialization;

namespace MeerkatDotnet.Services;

class LoginFailedException : Exception
{
    public LoginFailedException()
    {
    }

    public LoginFailedException(string? message) : base(message)
    {
    }

    public LoginFailedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected LoginFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
