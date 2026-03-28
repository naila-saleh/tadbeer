using System;

namespace Tadbeer.BLL.Exceptions;

public class UserOperationException : Exception
{
    public UserOperationException(string message) : base(message)
    {
    }
}
