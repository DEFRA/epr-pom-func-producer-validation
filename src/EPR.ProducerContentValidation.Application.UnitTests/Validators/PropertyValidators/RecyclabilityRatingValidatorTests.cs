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
            .WithErrorCode(ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(true, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible)]
    [DataRow(false, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible)]
    [DataRow(true, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(false, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(true, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "")]
    [DataRow(false, DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "")]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_Validation_Varies_BasedOn_FeatureFlag(
        bool isFeatureFlagEnabled,
        string dataSubmissionPeriod,
        string producerType,
        string producerSize,
        string packagingType,
        string packagingClass,
        string materialType,
        string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null);
        var context = CreateContextWithFeatureFlag(producerRow, isFeatureFlagEnabled);

        // Act
        var result = _systemUnderTest.TestValidate(context);

        // Assert
        if (isFeatureFlagEnabled)
        {
            result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
                 .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired);
        }
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
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Plastic, null, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Other, "test", RecyclabilityRating.Green)]
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
    [DataRow(true, "Z")]
    [DataRow(false, "INVALID")]
    public void InvalidRecyclabilityRating_ShouldRaiseError_WhenPresent(bool isFlagOn, string rating)
    {
        var row = BuildProducerRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, string.Empty, rating);
        var context = CreateContextWithFeatureFlag(row, isFlagOn);

        var result = _systemUnderTest.TestValidate(context);

        var expectedErrorCode = isFlagOn ? ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode : ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode;

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(expectedErrorCode);
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
    [DataRow(true, "2025-H1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Glass, "", "Invalid", ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)]
    [DataRow(true, "2025-H2", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Aluminium, "", "123", ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)]
    [DataRow(true, "2025-H2", "L", PackagingType.HouseholdDrinksContainers, "", MaterialType.Glass, "", "Fake", ErrorCode.LargeProducerEnhancedRecyclabilityRatingValidationInvalidErrorCode)]
    [DataRow(false, "2025-H1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Glass, "", "Invalid", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    [DataRow(false, "2025-H2", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Aluminium, "", "123", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    [DataRow(false, "2025-H2", "L", PackagingType.HouseholdDrinksContainers, "", MaterialType.Glass, "", "Fake", ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode)]
    public void Should_Fail_When_InvalidRecyclabilityCode_WithCorrectErrorCodeBasedOnFlag(
        bool featureFlagOn,
        string period,
        string orgSize,
        string pkgType,
        string pkgClass,
        string material,
        string subType,
        string rating,
        string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, rating);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = featureFlagOn;

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
    [DataRow("2023-P1", "L", PackagingType.HouseholdDrinksContainers, "", MaterialType.Glass, "", RecyclabilityRating.Red, ErrorCode.LargeProducerRecyclabilityRatingNotRequired)]
    [DataRow("2023-P1", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Glass, "", RecyclabilityRating.Red, ErrorCode.LargeProducerRecyclabilityRatingNotRequired)]
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

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.SelfManagedConsumerWaste, MaterialType.Plastic)]
    [DataRow(ProducerSize.Large, PackagingType.SelfManagedOrganisationWaste, MaterialType.Aluminium)]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Plastic)]
    [DataRow(ProducerSize.Large, PackagingType.SmallOrganisationPackagingAll, MaterialType.Steel)]
    [DataRow(ProducerSize.Small, PackagingType.PublicBin, MaterialType.Glass, ErrorCode.SmallProducerRecyclabilityRatingNotRequired)]
    public void Should_Fail_When_RecyclabilityRating_Provided_For_InvalidWasteAndMaterialType_WhenFlagEnabled(string producerSize, string packagingType, string materialType, string errorCode = ErrorCode.LargeProducerInvalidForWasteAndMaterialType)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriod.Year2025H1,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: MaterialSubType.Rigid,
            recyclabilityRating: RecyclabilityRating.Red);

        var context = CreateContextWithFeatureFlag(row, isEnabled: true);

        var result = _systemUnderTest.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(errorCode);
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic)]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Steel)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.Aluminium)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.Glass)]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass)]
    public void Should_Pass_When_RecyclabilityRating_Provided_For_ValidWasteAndMaterialType_WhenFlagEnabled(string producerSize, string packagingType, string materialType)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriod.Year2025H1,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: MaterialSubType.Rigid,
            recyclabilityRating: RecyclabilityRating.Green);

        var context = CreateContextWithFeatureFlag(row, isEnabled: true);

        var result = _systemUnderTest.TestValidate(context);

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
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