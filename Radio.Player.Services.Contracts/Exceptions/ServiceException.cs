namespace Radio.Player.Services.Contracts.Exceptions;

public class ServiceException : Exception
{
    public string ServiceTypeName { get; set; }

    public ServiceException(string serviceTypeName)
    {
        ServiceTypeName = serviceTypeName;
    }
    
    public ServiceException(string serviceTypeName, string? message) : base(message)
    {
        ServiceTypeName = serviceTypeName;
    }

    public ServiceException(string serviceTypeName, string? message, Exception? innerException) : base(message, innerException)
    {
        ServiceTypeName = serviceTypeName;
    }
}