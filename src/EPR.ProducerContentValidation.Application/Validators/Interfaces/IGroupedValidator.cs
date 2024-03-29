﻿using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.Application.Validators.Interfaces;

public interface IGroupedValidator
{
    Task ValidateAndAddErrorsAsync(List<ProducerRow> producerRows, List<ProducerValidationEventIssueRequest> errorRows, List<ProducerValidationEventIssueRequest> warningRows, string blobName);
}