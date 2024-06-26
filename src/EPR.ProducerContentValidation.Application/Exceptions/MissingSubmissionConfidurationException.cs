using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Exceptions;

[ExcludeFromCodeCoverage]
[Serializable]
public class MissingSubmissionConfidurationException : Exception
{
    public MissingSubmissionConfidurationException(string message)
    : base(message)
    {
    }
}
