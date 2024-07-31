namespace EPR.ProducerContentValidation.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
[Serializable]
public class SubmissionApiClientException : Exception
{
    public SubmissionApiClientException(string message, Exception cause)
        : base(message, cause)
    {
    }
}