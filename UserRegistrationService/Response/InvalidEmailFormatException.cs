using System.Runtime.Serialization;

namespace UserManagement.Controllers
{
    [Serializable]
    public class InvalidEmailFormatException : Exception
    {
        public InvalidEmailFormatException()
        {
        }

        public InvalidEmailFormatException(string? message) : base(message)
        {
        }

        public InvalidEmailFormatException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidEmailFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}