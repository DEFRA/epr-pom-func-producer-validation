namespace EPR.ProducerContentValidation.Application.Models;

public record Producer(
    Guid SubmissionId,
    string ProducerId,
    string BlobName,
    List<ProducerRow> Rows);