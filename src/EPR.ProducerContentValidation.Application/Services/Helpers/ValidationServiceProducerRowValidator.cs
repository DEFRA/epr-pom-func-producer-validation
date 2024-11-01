using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Helpers
{
    public class ValidationServiceProducerRowValidator : IValidationServiceProducerRowValidator
    {
        private readonly IFindMatchingProducer _findMatchingProducer;

        public ValidationServiceProducerRowValidator(IFindMatchingProducer findMatchingProducer)
        {
            _findMatchingProducer = findMatchingProducer;
        }

        public IEnumerable<ProducerValidationEventIssueRequest> ProcessRowsForValidationErrors(
            List<ProducerRow> rows, SubsidiaryDetailsResponse response)
        {
            var validationErrors = new List<ProducerValidationEventIssueRequest>();

            for (var i = 0; i < rows.Count; i++)
            {
                var error = _findMatchingProducer.Match(rows[i], response, i);
                if (error != null)
                {
                    validationErrors.Add(error);
                }
            }

            return validationErrors;
        }
    }
}
