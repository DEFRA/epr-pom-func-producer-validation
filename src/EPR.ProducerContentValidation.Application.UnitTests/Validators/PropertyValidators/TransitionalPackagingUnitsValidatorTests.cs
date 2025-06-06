namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class TransitionalPackagingUnitsValidatorTests
{
    private TransitionalPackagingUnitsValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new TransitionalPackagingUnitsValidator();
    }

    [TestMethod]
    [DataRow("2024P1", "100")]
    [DataRow("2024-H2", "1")]
    public void TransitionalPackagingUnitsValidator_ShouldNotHaveValidationError_WhenValidUnitsAndPeriodIs2024(string period, string units)
    {
        var model = BuildProducerRow(period, units);
        var result = _systemUnderTest.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.TransitionalPackagingUnits);
    }

    [TestMethod]
    [DataRow("2024P1", null)]
    [DataRow("2024P1", "1")]
    public void TransitionalPackagingUnitsValidator_ShouldNotHaveValidationError_WhenEmptyValueAndPeriodIs2024(string period, string units)
    {
        var model = BuildProducerRow(period, units);
        var result = _systemUnderTest.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.TransitionalPackagingUnits);
    }

    [TestMethod]
    [DataRow("2025P1", "100")]
    [DataRow("2026-H1", "1")]
    [DataRow("2023-H2", "99")]
    public void TransitionalPackagingUnitsValidator_ShouldHaveErrorCode91_WhenNon2024PeriodWithValue(string period, string units)
    {
        var model = BuildProducerRow(period, units);
        var result = _systemUnderTest.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.TransitionalPackagingUnits)
              .WithErrorCode(ErrorCode.TransitionalPackagingUnitsNotAllowedForThisPeriod);
    }

    [TestMethod]
    [DataRow("2024P1", "-1")]
    [DataRow("2024P1", "notanumber")]
    public void TransitionalPackagingUnitsValidator_ShouldHaveErrorCode90_WhenValueIsInvalidFormatOrNegative(string period, string units)
    {
        var model = BuildProducerRow(period, units);
        var result = _systemUnderTest.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.TransitionalPackagingUnits)
              .WithErrorCode(ErrorCode.TransitionalPackagingUnitsInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string? transitionalPackagingUnits)
    {
        return new ProducerRow(
            SubsidiaryId: null,
            DataSubmissionPeriod: dataSubmissionPeriod,
            ProducerId: null,
            RowNumber: 1,
            ProducerType: null,
            ProducerSize: null,
            WasteType: null,
            PackagingCategory: null,
            MaterialType: null,
            MaterialSubType: null,
            FromHomeNation: null,
            ToHomeNation: null,
            QuantityKg: null,
            QuantityUnits: null,
            SubmissionPeriod: null,
            TransitionalPackagingUnits: transitionalPackagingUnits,
            RecyclabilityRating: null);
    }
}