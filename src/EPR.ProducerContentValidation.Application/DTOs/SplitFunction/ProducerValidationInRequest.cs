namespace EPR.ProducerContentValidation.Application.DTOs.SplitFunction;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class ProducerValidationInRequest
{
    public Guid OrganisationId { get; set; }

    public Guid UserId { get; set; }

    public Guid SubmissionId { get; set; }

    public string BlobName { get; set; }

    public string ProducerId { get; set; }

    public List<ProducerRowInRequest> Rows { get; set; }
}
