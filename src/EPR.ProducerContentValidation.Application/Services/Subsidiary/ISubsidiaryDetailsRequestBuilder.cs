﻿using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;

namespace EPR.ProducerContentValidation.Application.Services.Subsidiary
{
    public interface ISubsidiaryDetailsRequestBuilder
    {
        SubsidiaryDetailsRequest CreateRequest(List<ProducerRow> rows);
    }
}