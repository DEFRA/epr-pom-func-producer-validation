namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators.WarningValidators;

using Application.Validators.PropertyValidators.WarningValidators;
using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class RecyclabilityRatingUnlikelyCombinationsValidatorTests : RecyclabilityRatingUnlikelyCombinationsValidator
{
    private readonly RecyclabilityRatingUnlikelyCombinationsValidator _systemUnderTest;

    public RecyclabilityRatingUnlikelyCombinationsValidatorTests()
    {
        _systemUnderTest = new RecyclabilityRatingUnlikelyCombinationsValidator();
    }

    [TestMethod]
    [DataRow(PackagingType.Household, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Green)]
    [DataRow(PackagingType.Household, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]
    [DataRow(PackagingType.Household, MaterialType.Wood, "", RecyclabilityRating.Green)]
    [DataRow(PackagingType.Household, MaterialType.Wood, "", RecyclabilityRating.GreenMedical)]
    [DataRow(PackagingType.Household, MaterialType.Wood, "", RecyclabilityRating.AmberMedical)]
    [DataRow(PackagingType.Household, MaterialType.Other, "", RecyclabilityRating.GreenMedical)]
    [DataRow(PackagingType.Household, MaterialType.Other, "", RecyclabilityRating.AmberMedical)]
    [DataRow(PackagingType.PublicBin, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Green)]
    [DataRow(PackagingType.PublicBin, MaterialType.Wood, "", RecyclabilityRating.AmberMedical)]
    [DataRow(PackagingType.PublicBin, MaterialType.Other, "", RecyclabilityRating.GreenMedical)]
    public void Should_Fail_When_LargeProducerFrom2025_Has_UnlikelyMaterialRatingCombination_On_EligibleWaste(
        string packagingType,
        string materialType,
        string materialSubType,
        string recyclabilityRating)
    {
        var row = BuildProducerRow(
            packagingType: packagingType,
            producerSize: ProducerSize.Large,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingPresentForUnlikelyCombinations);
    }

    [TestMethod]
    [DataRow(PackagingType.NonHousehold, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Green)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, MaterialType.Wood, "", RecyclabilityRating.AmberMedical)]
    [DataRow(PackagingType.SelfManagedOrganisationWaste, MaterialType.Other, "", RecyclabilityRating.GreenMedical)]
    [DataRow(PackagingType.ReusablePackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]
    [DataRow(PackagingType.HouseholdDrinksContainers, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Green)]
    public void Should_Not_Fail_When_UnlikelyMaterialRatingCombination_On_IneligibleWaste(
        string packagingType,
        string materialType,
        string materialSubType,
        string recyclabilityRating)
    {
        var row = BuildProducerRow(
            packagingType: packagingType,
            producerSize: ProducerSize.Large,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Amber)]
    [DataRow(MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(MaterialType.Wood, "", RecyclabilityRating.Red)]
    [DataRow(MaterialType.Wood, "", RecyclabilityRating.Amber)]
    [DataRow(MaterialType.Other, "", RecyclabilityRating.Red)]
    [DataRow(MaterialType.Other, "", RecyclabilityRating.Green)]
    [DataRow(MaterialType.Other, "", RecyclabilityRating.Amber)]
    [DataRow(MaterialType.Glass, "", RecyclabilityRating.Green)]
    [DataRow(MaterialType.Steel, "", RecyclabilityRating.GreenMedical)]
    [DataRow(MaterialType.Aluminium, "", RecyclabilityRating.AmberMedical)]
    [DataRow(MaterialType.PaperCard, "", RecyclabilityRating.Green)]
    public void Should_Not_Fail_When_Combination_Is_Not_Unlikely(
        string materialType,
        string materialSubType,
        string recyclabilityRating)
    {
        var row = BuildProducerRow(
            producerSize: ProducerSize.Large,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    public void Should_Not_Fail_When_RecyclabilityRating_Is_Empty()
    {
        var row = BuildProducerRow(
            producerSize: ProducerSize.Large,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: string.Empty);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    public void Should_Not_Fail_When_Producer_Is_Small()
    {
        var row = BuildProducerRow(
            producerSize: ProducerSize.Small,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    public void Should_Not_Fail_When_SubmissionPeriod_Is_Before2025()
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2024P1,
            producerSize: ProducerSize.Large,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsCLR()
    {
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(
            packagingType: PackagingType.ClosedLoopRecycling,
            producerSize: ProducerSize.Large,
            materialType: MaterialType.Plastic,
            materialSubType: MaterialSubType.Flexible,
            recyclabilityRating: RecyclabilityRating.Green);
        var context = new ValidationContext<ProducerRow>(producerRow);

        var result = PreValidate(context, validationResult);

        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(
        string dataSubmissionPeriod = DataSubmissionPeriodTestData.Year2025H1,
        string producerSize = ProducerSize.Large,
        string packagingType = PackagingType.Household,
        string materialType = MaterialType.Plastic,
        string materialSubType = MaterialSubType.Rigid,
        string recyclabilityRating = RecyclabilityRating.Green)
    {
        return new ProducerRow(
            SubsidiaryId: null,
            DataSubmissionPeriod: dataSubmissionPeriod,
            ProducerId: null,
            SubmissionPeriod: null,
            RowNumber: 1,
            ProducerType: ProducerType.SuppliedUnderYourBrand,
            ProducerSize: producerSize,
            WasteType: packagingType,
            PackagingCategory: PackagingClass.PrimaryPackaging,
            MaterialType: materialType,
            MaterialSubType: materialSubType,
            FromHomeNation: null,
            ToHomeNation: null,
            QuantityKg: null,
            QuantityUnits: null,
            TransitionalPackagingUnits: null,
            RecyclabilityRating: recyclabilityRating);
    }
}