namespace EPR.ProducerContentValidation.Application.Exceptions;

public class MissingSubmissionConfidurationException : Exception
{
    public MissingSubmissionConfidurationException(string message)
    : base(message)
    {
    }
}
