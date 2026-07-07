namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class RecyclabilityRatingValidatorTests : RecyclabilityRatingValidator
{
    private readonly RecyclabilityRatingValidator _systemUnderTest;

    public RecyclabilityRatingValidatorTests()
    {
        _systemUnderTest = new RecyclabilityRatingValidator();
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, MaterialSubType.PET, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Amber)]
    [DataRow(DataSubmissionPeriodTestData.Year2025H2, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]
    [DataRow(DataSubmissionPeriodTestData.Year2028H2, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]

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
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "", "S")]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", "K-D")]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, "Z")]
    public void LargeProducerRecyclabilityRatingValidator_Invalid_Recyclability_Code(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(producerRow));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible)]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.PaperCard, "")]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Wood, "")]
    [DataRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, MaterialType.Glass, "")]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_Validation(
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

        // Act
        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(producerRow));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow("2023-P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriodTestData.Year2023P3, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow("2023P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2024P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", RecyclabilityRating.Green)]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_Not_Required_Before_2025(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(producerRow));

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
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.SecondaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TransitPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TotalPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Plastic, null, RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Other, "test", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2024P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Other, "test", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriodTestData.Year2027P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, null, MaterialType.Other, "test", RecyclabilityRating.Green)]
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
    [DataRow(ProducerSize.Large, PackagingType.SelfManagedConsumerWaste, MaterialType.Plastic, ErrorCode.LargeProducerInvalidForWasteAndMaterialType, DataSubmissionPeriodTestData.Year2025H2)]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Plastic, ErrorCode.LargeProducerInvalidForWasteAndMaterialType, DataSubmissionPeriodTestData.Year2025H1)]
    [DataRow(ProducerSize.Small, PackagingType.PublicBin, MaterialType.Glass, ErrorCode.SmallProducerRecyclabilityRatingNotRequired)]
    public void Should_Fail_When_RecyclabilityRating_Provided_For_InvalidWasteAndMaterialType(
        string producerSize,
        string packagingType,
        string materialType,
        string errorCode = ErrorCode.LargeProducerInvalidForWasteAndMaterialType,
        string dataSubmissionPeriod = DataSubmissionPeriodTestData.Year2025H1)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: dataSubmissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: MaterialSubType.Rigid,
            recyclabilityRating: RecyclabilityRating.Red);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        Assert.AreEqual(1, result.Errors.Count);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
              .WithErrorCode(errorCode);
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, DataSubmissionPeriodTestData.Year2025P0, " A")]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, DataSubmissionPeriodTestData.Year2025P0, "A ")]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, DataSubmissionPeriodTestData.Year2025P0, "A_M")]
    public void Should_Fail_When_InvalidRecyclabilityRating_Provided_After_2024(string producerSize, string packagingType, string materialType, string submissionPeriod, string recyclabilityRating)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: submissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: string.Empty,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        Assert.AreEqual(1, result.Errors.Count);

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
             .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidValue);
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic)]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Steel)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.Aluminium)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.Glass)]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass)]
    public void Should_Pass_When_RecyclabilityRating_Provided_For_ValidWasteAndMaterialType(string producerSize, string packagingType, string materialType)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2025H1,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: MaterialSubType.Rigid,
            recyclabilityRating: RecyclabilityRating.Green);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, "", DataSubmissionPeriodTestData.Year2025H1, "A")]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.PaperCard, "", DataSubmissionPeriodTestData.Year2025H1, "A")]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic, MaterialSubType.Rigid, DataSubmissionPeriodTestData.Year2025H1, "A")]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic, MaterialSubType.Flexible,  DataSubmissionPeriodTestData.Year2025H1, "A")]
    public void Should_Pass_When_Valid_But_Optional_RecyclabilityRating_Provided_During_2025H1_Only(string producerSize, string packagingType, string materialType, string materialSubType, string submissionPeriod, string recyclabilityRating)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: submissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, "", DataSubmissionPeriodTestData.Year2025H1)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.PaperCard, "", DataSubmissionPeriodTestData.Year2025H1)]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic, MaterialSubType.Rigid, DataSubmissionPeriodTestData.Year2025H1)]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic, MaterialSubType.Flexible,  DataSubmissionPeriodTestData.Year2025H1)]
    public void Should_Pass_When_No_Rating_Supplied_For_Matching_MaterialTypes_During_2025H1_Only(string producerSize, string packagingType, string materialType, string materialSubType, string submissionPeriod)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: submissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: string.Empty);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Amber)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Wood, "", RecyclabilityRating.Red)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Other, "", RecyclabilityRating.Green)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Wood, "", RecyclabilityRating.GreenMedical)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Other, "", RecyclabilityRating.AmberMedical)]
    [DataRow(ProducerSize.Large, PackagingType.NonHousehold, MaterialType.Glass, "", RecyclabilityRating.Red)]
    [DataRow(ProducerSize.Large, PackagingType.ReusablePackaging, MaterialType.Steel, "", RecyclabilityRating.Red)]
    [DataRow(ProducerSize.Large, PackagingType.NonHouseholdDrinksContainers, MaterialType.Aluminium, "", RecyclabilityRating.Red)]
    [DataRow(ProducerSize.Large, PackagingType.SelfManagedOrganisationWaste, MaterialType.PaperCard, "", RecyclabilityRating.Amber)]
    [DataRow(ProducerSize.Large, PackagingType.SelfManagedConsumerWaste, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Amber)]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Red)]
    public void Should_Fail_When_RecyclabilityRating_Provided_For_IneligibleWaste(
        string producerSize,
        string packagingType,
        string materialType,
        string materialSubType,
        string recyclabilityRating)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2025H1,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: recyclabilityRating);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerInvalidForWasteAndMaterialType);
    }

    [TestMethod]
    [DataRow(PackagingType.NonHousehold, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(PackagingType.SelfManagedConsumerWaste, MaterialType.Wood, "")]
    [DataRow(PackagingType.ReusablePackaging, MaterialType.Steel, "")]
    [DataRow(PackagingType.HouseholdDrinksContainers, MaterialType.Plastic, MaterialSubType.Rigid)]
    public void Should_Not_Fail_When_Rating_Is_Missing_On_IneligibleWaste(
        string packagingType,
        string materialType,
        string materialSubType)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2025H1,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: ProducerSize.Large,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: string.Empty);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(ProducerSize.Large, PackagingType.Household, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(ProducerSize.Large, PackagingType.PublicBin, MaterialType.PaperCard, "")]
    [DataRow(ProducerSize.Large, PackagingType.HouseholdDrinksContainers, MaterialType.Glass, "")]
    public void Should_Not_Fail_When_RecyclabilityRating_Is_Missing_For_EligibleWasteMaterial_Because_GroupedValidators_Handle_MissingRatings(
        string producerSize,
        string packagingType,
        string materialType,
        string materialSubType)
    {
        var row = BuildProducerRow(
            dataSubmissionPeriod: DataSubmissionPeriodTestData.Year2025H2,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: producerSize,
            packagingType: packagingType,
            packagingClass: PackagingClass.PrimaryPackaging,
            materialType: materialType,
            materialSubType: materialSubType,
            recyclabilityRating: string.Empty);

        var result = _systemUnderTest.TestValidate(new ValidationContext<ProducerRow>(row));

        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenPackagingTypeIsCLR()
    {
        // Arrange
        var validationResult = new ValidationResult();
        var producerRow = BuildProducerRow(DataSubmissionPeriodTestData.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.ClosedLoopRecycling, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, string.Empty, RecyclabilityRating.Red);
        var context = new ValidationContext<ProducerRow>(producerRow);

        // Act
        var result = PreValidate(context, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null, null, null, null, null, null, recyclabilityRating);
    }
}