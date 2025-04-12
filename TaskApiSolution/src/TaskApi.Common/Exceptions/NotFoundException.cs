using TaskApi.Common.Exceptions.Base;

namespace TaskApi.Common.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404) { }
    }
}
