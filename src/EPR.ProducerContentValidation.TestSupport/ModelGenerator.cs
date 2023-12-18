using EPR.ProducerContentValidation.Application.Models;

namespace EPR.ProducerContentValidation.TestSupport;

public static class ModelGenerator
{
    public static ProducerRow CreateProducerRow(int rowNumber)
    {
        return new ProducerRow(
            "SubsidiaryId",
            "DataSubmissionPeriod",
            "000123",
            rowNumber,
            "ProducerType",
            "ProducerSize",
            "WasteType",
            "Category",
            "MaterialType",
            "MaterialSubType",
            "FromHomeNation",
            "ToHomeNation",
            "1",
            "1",
            "SubmissionPeriod");
    }
}