using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;

namespace EPR.ProducerContentValidation.Application.Services.Helpers;

public class FindMatchingProducer : IFindMatchingProducer
{
    private readonly IOrganisationMatcher _organisationMatcher;
    private readonly ISubsidiaryMatcher _subsidiaryMatcher;
    private readonly ISubsidiaryValidationEvaluator _subsidiaryValidationEvaluator;

    public FindMatchingProducer(IOrganisationMatcher organisationMatcher, ISubsidiaryMatcher subsidiaryMatcher, ISubsidiaryValidationEvaluator subsidiaryValidationEvaluator)
    {
        _organisationMatcher = organisationMatcher;
        _subsidiaryMatcher = subsidiaryMatcher;
        _subsidiaryValidationEvaluator = subsidiaryValidationEvaluator;
    }

    public ProducerValidationEventIssueRequest? Match(
        ProducerRow row, SubsidiaryDetailsResponse response, int rowIndex, string blobName)
    {
        var matchingOrg = _organisationMatcher.FindMatchingOrganisation(row, response);
        if (matchingOrg == null)
        {
            return null;
        }

        var matchingSub = _subsidiaryMatcher.FindMatchingSubsidiary(row, matchingOrg);
        if (matchingSub == null)
        {
            return null;
        }

        return _subsidiaryValidationEvaluator.EvaluateSubsidiaryValidation(row, matchingSub, rowIndex, blobName);
    }
}
