namespace Simplified_Threat_Intelligence_Platform.Shared
{
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
