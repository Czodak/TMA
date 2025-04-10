using AuthApi.Exceptions.Base;

namespace AuthApi.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }
}
