using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace EPR.ProducerContentValidation.Application.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class MissingSubmissionConfidurationException : Exception
{
    public MissingSubmissionConfidurationException(string message)
    : base(message)
    {
    }

    protected MissingSubmissionConfidurationException(SerializationInfo info, StreamingContext context)
    : base(info, context)
    {
    }
}
