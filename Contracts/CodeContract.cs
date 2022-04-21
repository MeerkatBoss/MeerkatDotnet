using System.Diagnostics;
using System.Reflection;
using log4net;

namespace MeerkatDotnet.Contracts;

public static class CodeContract
{
    private static readonly ILog log = LogManager.GetLogger(typeof(CodeContract));
    public static void Requires<TException>(bool requirement, string? message = null)
        where TException : Exception, new()
    {
        if (!requirement)
        {
            if (message is null)
            {
                var emptyException = new TException();
                log.Error(emptyException.ToString());
                throw emptyException;
            }
            var type = typeof(TException);

            var ctor = type.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(string) });

            if (ctor is null)
            {
                throw new InvalidOperationException(
                    "Cannot specify message for exception "
                    + "class without Exception(string) constructor");
            }
            var exception = ctor.Invoke(new object[] { message }) as TException;
            log.Error(exception!.ToString());
            throw exception;
        }
    }
}