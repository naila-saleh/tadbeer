namespace Tadbeer.BLL.Exceptions;

public class DuplicateSpecialtyException : Exception
{
    public DuplicateSpecialtyException(string message) : base(message)
    {
    }
}