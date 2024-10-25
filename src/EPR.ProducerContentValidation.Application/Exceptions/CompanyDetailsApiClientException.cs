using System.Diagnostics.CodeAnalysis;

namespace EPR.ProducerContentValidation.Application.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class CompanyDetailsApiClientException : Exception
    {
        public CompanyDetailsApiClientException()
        {
        }

        public CompanyDetailsApiClientException(string message)
            : base(message)
        {
        }

        public CompanyDetailsApiClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
