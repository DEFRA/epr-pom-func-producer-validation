using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Exceptions;

[ExcludeFromCodeCoverage]
public class MissingSubmissionConfidurationException : Exception
{
    public MissingSubmissionConfidurationException(string message)
    : base(message)
    {
    }
}
