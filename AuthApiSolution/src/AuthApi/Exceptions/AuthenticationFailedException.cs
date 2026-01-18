using AuthApi.Exceptions.Base;
using System.Net;

namespace AuthApi.Exceptions;

public sealed class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException(string message) : base(message, (int)HttpStatusCode.Unauthorized) { }
}