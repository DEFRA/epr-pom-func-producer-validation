using EPR.ProducerContentValidation.Application.DTOs.SplitFunction;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

namespace EPR.ProducerContentValidation.TestSupport;

public static class DtoGenerator
{
    public static ProducerValidationInRequest ValidProducerValidationInRequest()
    {
        var producerRows = new List<ProducerRowInRequest>
        {
            new(
                "SubsidiaryId",
                "2023-P1",
                1,
                "000123",
                "ProducerType",
                "S",
                "WasteType",
                "PackagingCategory",
                "MaterialType",
                "MaterialSubType",
                "FromHomeNation",
                "ToHomeNation",
                "1",
                "1",
                "2023P3")
        };

        return new ProducerValidationInRequest
        {
            OrganisationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            SubmissionId = Guid.NewGuid(),
            ProducerId = "123456",
            Rows = producerRows
        };
    }

    public static SubmissionEventRequest InvalidProducerValidationOutRequest()
    {
        var errorRows = new List<ProducerValidationEventIssueRequest>
        {
            new(
                "SubsidiaryId",
                "2023-P1",
                1,
                "123456",
                "ProducerType",
                "ProducerSize",
                "WasteType",
                "PackagingCategory",
                "MaterialType",
                "MaterialSubType",
                "FromHomeNation",
                "ToHomeNation",
                "1",
                "1",
                "blobName",
                new List<string> { "Error" })
        };

        return new SubmissionEventRequest("BlobName", "BlobContainerName", "123456", null, errorRows);
    }
}