namespace EPR.ProducerContentValidation.Application.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[ExcludeFromCodeCoverage]
[Serializable]
public class SubmissionApiClientException : Exception
{
    public SubmissionApiClientException(string message, Exception cause)
        : base(message, cause)
    {
    }

    protected SubmissionApiClientException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}