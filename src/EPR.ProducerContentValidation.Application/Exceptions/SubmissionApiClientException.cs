namespace EPR.ProducerContentValidation.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionApiClientException : Exception
{
    public SubmissionApiClientException(string message, Exception cause)
        : base(message, cause)
    {
    }
}