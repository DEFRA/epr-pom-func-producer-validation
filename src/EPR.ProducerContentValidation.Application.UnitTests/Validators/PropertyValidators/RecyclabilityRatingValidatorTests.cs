namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;

[TestClass]
public class RecyclabilityRatingValidatorTests : RecyclabilityRatingValidator
{
    private readonly RecyclabilityRatingValidator _systemUnderTest;
    private Mock<IFeatureManager> _featureManagerMock;

    public RecyclabilityRatingValidatorTests()
    {
        _systemUnderTest = new RecyclabilityRatingValidator();
        _featureManagerMock = new Mock<IFeatureManager>();
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, MaterialSubType.PET, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Amber)]
    [DataRow(DataSubmissionPeriod.Year2025H2, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]
    public void LargeProducerRecyclabilityRatingValidator_Recyclability_Code_ValidSubmission(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", RecyclabilityRating.Red)]
    public void LargeProducerRecyclabilityRatingValidator_Recyclability_Code_packaging_material_subtype_missing1(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Other, "", RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Amber)]
    [DataRow(DataSubmissionPeriod.Year2025H2, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.ShipmentPackaging, MaterialType.Plastic, "", RecyclabilityRating.GreenMedical)]
    public void LargeProducerRecyclabilityRatingValidator_Recyclability_Code_packaging_material_subtype_missing(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", "S")]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", "K-D")]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, "Z")]
    public void LargeProducerRecyclabilityRatingValidator_Invalid_Recyclability_Code(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(CreateContextWithFeatureFlag(producerRow));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "")]
    public void LargeProducerRecyclabilityRatingValidator_HH_Missing_Recyclability_Code(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null);

        // Act
        var result = _systemUnderTest.TestValidate(CreateContextWithFeatureFlag(producerRow, isEnabled: true));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible, "")]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, "")]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(CreateContextWithFeatureFlag(producerRow, isEnabled: true));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2023P3, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow("2023P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow("2023P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_Not_Required_Before_2025(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(CreateContextWithFeatureFlag(producerRow));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired);
    }

    [TestMethod]
    [DataRow("P023P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_When_SubmissionPeriod_Not_Valid(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.SecondaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TransitPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    public void RecyclabilityRatingValidator_Recyclability_Code_Not_Required_For_SmallProducer(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
       .WithErrorCode(ErrorCode.SmallProducerRecyclabilityRatingNotRequired);
    }

    [TestMethod]
    [DataRow(true, DataSubmissionPeriod.Year2025H1, "GL", "", false)]
    [DataRow(false, DataSubmissionPeriod.Year2025H1, "GL", "", true)]
    public void RecyclabilityRating_OptionalOrRequired_BasedOnFlag(bool isFlagOn, string submissionPeriod, string materialType, string rating, bool featureFlagEnabled)
    {
        var row = BuildProducerRow(submissionPeriod, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, materialType, string.Empty, rating);
        var context = CreateContextWithFeatureFlag(row, isFlagOn);

        var result = _systemUnderTest.TestValidate(CreateContextWithFeatureFlag(row, isEnabled: featureFlagEnabled));

        if (featureFlagEnabled)
        {
            result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
                  .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
        }
    }

    [TestMethod]
    [DataRow(true, "Z")]
    [DataRow(false, "INVALID")]
    public void InvalidRecyclabilityRating_ShouldRaiseError_WhenPresent(bool isFlagOn, string rating)
    {
        var row = BuildProducerRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, string.Empty, rating);
        var context = CreateContextWithFeatureFlag(row, isFlagOn);

        var result = _systemUnderTest.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void RecyclabilityRating_NotRequiredBefore2025_ShouldRaiseError_WhenProvided(bool isFlagOn)
    {
        var row = BuildProducerRow("2024-P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, string.Empty, RecyclabilityRating.Green);
        var context = CreateContextWithFeatureFlag(row, isFlagOn);

        var result = _systemUnderTest.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired);
    }

    [TestMethod]
    [DataRow("2025-H1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Glass, "", "Invalid", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    [DataRow("2025-H2", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Aluminium, "", "123", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    [DataRow("2025-H2", "L", PackagingType.HouseholdDrinksContainers, "", MaterialType.Glass, "", "Fake", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    public void Should_Fail_When_InvalidRecyclabilityCode(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string rating, string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, rating);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = true;

        var validator = new RecyclabilityRatingValidator();
        var result = validator.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(expectedError);
    }

    [TestMethod]
    [DataRow("2024-P1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    public void Should_Fail_When_PlasticSubtypeProvided_Before2025(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, null);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = true;

        var validator = new MaterialSubMaterialCombinationValidator();
        var result = validator.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
              .WithErrorCode(expectedError);
    }

    [TestMethod]
    [DataRow("2023-P1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Glass, "", RecyclabilityRating.Red, ErrorCode.LargeProducerRecyclabilityRatingNotRequired)]
    public void Should_Fail_When_RecyclabilityRatingProvided_Before2025(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string rating, string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, rating);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = true;

        var validator = new RecyclabilityRatingValidator();
        var result = validator.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(expectedError);
    }

    private static ProducerRow BuildProducerRow(string producerSize)
    {
        return new ProducerRow(null, null, null, 1, null, producerSize, null, null, null, null, null, null, null, null, null, null, null);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null, null, null, null, null, null, recyclabilityRating);
    }

    private static ValidationContext<ProducerRow> CreateContextWithFeatureFlag(ProducerRow row, bool isEnabled = true)
    {
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = isEnabled;
        return context;
    }
}