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
public class MaterialSubMaterialCombinationValidatorTests : MaterialSubMaterialCombinationValidator
{
    private readonly MaterialSubMaterialCombinationValidator _systemUnderTest;

    public MaterialSubMaterialCombinationValidatorTests()
    {
        _systemUnderTest = new MaterialSubMaterialCombinationValidator();
    }

    [TestMethod]
    [DataRow("ABCD1EFG")]
    [DataRow("ABCD,EFG")]
    [DataRow("ABCDEFG1")]
    [DataRow("ABCDEFG,")]
    [DataRow("1ABCDEFG")]
    [DataRow(",ABCDEFG")]
    [DataRow("123")]
    [DataRow(",")]
    public void MaterialSubMaterialCombinationValidator_ContainErrorForMaterialSubType_WhenSubTypeContainsNumbersOrCommas(string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, materialSubType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType).WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);
    }

    [TestMethod]
    [DataRow("ABCDEFG")]
    [DataRow("ABCDEFG%!$%^&*()_+={}:@~<>?|./#")]
    public void MaterialSubMaterialCombinationValidator_DoesContainErrorForMaterialSubType_WhenSubTypeContainsNoNumbersOrCommas(string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, materialSubType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(MaterialType.Other)]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForMaterialCombination_WhenSubTypePresentAndMaterialIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(materialType, "MaterialSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
    }

    [TestMethod]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorWhenNoSubTypePresentForOther()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, string.Empty);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.OtherPackagingMaterialWithNoMaterialSubType);
    }

    [TestMethod]
    [DataRow(MaterialType.Wood)]
    [DataRow(MaterialType.Aluminium)]
    [DataRow(MaterialType.Steel)]
    [DataRow(MaterialType.Glass)]
    [DataRow(MaterialType.PaperCard)]
    [DataRow(MaterialType.FibreComposite)]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorForMaterialCombination_WhenSubTypePresentAndMaterialIs(string materialType)
    {
        // Arrange
        var producerRow = BuildProducerRow(materialType, "MaterialSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    public void PreValidate_ReturnsFalse_WhenInvalidMaterialSubMaterialCombinationInvalidErrorCodeIsPresent()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Plastic, "MaterialSubType");
        var validationContext = new ValidationContext<ProducerRow>(producerRow);
        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure
        {
            ErrorCode = ErrorCode.MaterialTypeInvalidErrorCode
        });

        // Act
        var result = PreValidate(validationContext, validationResult);

        // Assert
        result.Should().BeFalse();
    }

    [TestMethod]

    [DataRow(MaterialSubType.Plastic)]
    [DataRow(MaterialSubType.PET)]
    [DataRow(MaterialSubType.HDPE)]
    public void MaterialSubMaterialCombinationValidator_ContainsErrorForInvalidMaterialSubTypeForOther(string subType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, subType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.PackagingMaterialSubtypeInvalidForMaterialType);
    }

    [TestMethod]
    [DataRow("OtherSubtype")]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForValidMaterialSubTypesForOther(string subType)
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, subType);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    public void MaterialSubMaterialCombinationValidator_DoesNotContainErrorForValidMaterialTypeOtherAndValidMaterialSubType()
    {
        // Arrange
        var producerRow = BuildProducerRow(MaterialType.Other, "OtherValidSubType");

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.PET, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Wrong_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
             .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Missing_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
             .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_PackagingType_PublicBin_Missing_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
             .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_PackagingType_HDC_Missing_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // TO DO. HDC not required

        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_PackagingType_HD_Missing_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Plastic_Material_SubType_Not_Required_Before_2025(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
        .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.PET, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Packing_Type_HD_Invalid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Packing_Type_HDC_Invalid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.PET, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Packing_Type_PB_Invalid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Packing_Type_PB_InValid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.PET, RecyclabilityRating.Red)]
    public void MaterialSubMaterialCombinationValidator_Large_Producer_Packing_Type_PB_Valid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.SecondaryPackaging, MaterialType.Plastic, MaterialSubType.Plastic)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.SmallOrganisationPackagingAll, PackagingClass.TransitPackaging, MaterialType.Plastic, MaterialSubType.Plastic)]
    [DataRow(DataSubmissionPeriod.Year2025P0, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Flexible)]
    [DataRow(DataSubmissionPeriod.Year2024P3, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Flexible)]
    [DataRow(DataSubmissionPeriod.Year2024P3, ProducerType.SoldAsEmptyPackaging, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Flexible)]
    [DataRow(DataSubmissionPeriod.Year2023P3, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.HouseholdDrinksContainers, null, MaterialType.Plastic, MaterialSubType.Flexible)]
    public void MaterialSubMaterialCombinationValidator_SmallProducer_PlasticMaterial_SubType_NotRequired(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, bool shouldHaveValidationError = true)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null);

        var context = new ValidationContext<ProducerRow>(producerRow);
        context.RootContextData[FeatureFlags.EnableLargeProducerRecyclabilityRatingValidation] = false;

        // Act
        var result = _systemUnderTest.TestValidate(context);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
        .WithErrorCode(ErrorCode.SmallProducerPlasticMaterialSubTypeNotRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, ProducerType.SuppliedUnderYourBrand, ProducerSize.Small, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid)]
    public void MaterialSubMaterialCombinationValidator_SmallProducer_Plastic_Material_SubType_Not_Required_Before_2025(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
        .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    public void SubtypeBefore2025_ShouldRaiseError()
    {
        var row = BuildProducerRow("2024-P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, string.Empty);

        var result = _systemUnderTest.TestValidate(row);

        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
                .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    public void SubtypeBefore2025_ShouldRaiseError_For_PB()
    {
        var row = BuildProducerRow("2024-P1", ProducerType.SuppliedUnderYourBrand, ProducerSize.Large, PackagingType.PublicBin, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, string.Empty);

        var result = _systemUnderTest.TestValidate(row);

        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
                .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    [DataRow(PackagingType.Household, "", "2025-H1")]
    [DataRow(PackagingType.PublicBin, PackagingClass.PublicBin, "2025-H2")]
    public void MissingPlasticMaterialSubType_ShouldRaiseError_For_HH_PB(string packagingType, string packagingClass, string submissionPeriod)
    {
        // Arrange
        var producerRow = BuildProducerRow(
            dataSubmissionPeriod: submissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: ProducerSize.Large,
            packagingType: packagingType,
            packagingClass: packagingClass,
            materialType: MaterialType.Plastic,
            materialSubType: string.Empty,
            recyclabilityRating: RecyclabilityRating.Red);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
              .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired);
    }

    [TestMethod]
    [DataRow(PackagingType.HouseholdDrinksContainers, "", "2025-H1")]
    public void MissingPlasticMaterialSubType_ShouldRaise_NO_Error_For_HDC(string packagingType, string packagingClass, string submissionPeriod)
    {
        // TODO for HDC it should not throw error

        // Arrange
        var producerRow = BuildProducerRow(
            dataSubmissionPeriod: submissionPeriod,
            producerType: ProducerType.SuppliedUnderYourBrand,
            producerSize: ProducerSize.Large,
            packagingType: packagingType,
            packagingClass: packagingClass,
            materialType: MaterialType.Plastic,
            materialSubType: string.Empty,
            recyclabilityRating: RecyclabilityRating.Red);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow("2025-H1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", ErrorCode.LargeProducerPlasticMaterialSubTypeRequired)]
    [DataRow("2025-H1", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Plastic, "", ErrorCode.LargeProducerPlasticMaterialSubTypeRequired)]
    public void Should_Fail_When_PlasticMaterialSubtypeMissing(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string expectedError)
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
    [DataRow("2025-H2", "L", PackagingType.HouseholdDrinksContainers, "", MaterialType.Plastic, "", ErrorCode.LargeProducerPlasticMaterialSubTypeRequired)]
    public void Should_Not_Fail_When_PlasticMaterialSubtypeMissing(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, null);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = true;

        var validator = new MaterialSubMaterialCombinationValidator();
        var result = validator.TestValidate(context);

        result.ShouldNotHaveValidationErrorFor(x => x.MaterialType);
        result.ShouldNotHaveValidationErrorFor(x => x.MaterialSubType);
    }

    [TestMethod]
    [DataRow("2025-H1", "L", PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "Invalid", ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode)]
    [DataRow("2025-H1", "L", PackagingType.PublicBin, PackagingClass.PublicBin, MaterialType.Plastic, "PET", ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode)]
    public void Should_Fail_When_InvalidPlasticMaterialSubtype(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string expectedError)
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
    [DataRow("2025-H1", "L", PackagingType.NonHousehold, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Flexible, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    [DataRow("2025-H2", "L", PackagingType.NonHouseholdDrinksContainers, PackagingClass.PublicBin, MaterialType.Plastic, MaterialSubType.Plastic, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    [DataRow("2025-H1", "L", PackagingType.SelfManagedOrganisationWaste, PackagingClass.PublicBin, MaterialType.Plastic, MaterialSubType.Rigid, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    [DataRow("2025-H2", "L", PackagingType.SelfManagedConsumerWaste, PackagingClass.PublicBin, MaterialType.Plastic, "Random", ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    [DataRow("2025-H1", "L", PackagingType.ReusablePackaging, PackagingClass.PublicBin, MaterialType.Plastic, MaterialSubType.HDPE, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    [DataRow("2025-H2", "L", PackagingType.HouseholdDrinksContainers, PackagingClass.PublicBin, MaterialType.Plastic, MaterialSubType.Rigid, ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial)]
    public void Should_Fail_When_Packingtype_Is_Nonhousehold_And_MaterialSubtype_isnotempty(string period, string orgSize, string pkgType, string pkgClass, string material, string subType, string expectedError)
    {
        var row = new ProducerRow(null, period, null, 1, "SO", orgSize, pkgType, pkgClass, material, subType, null, null, null, null, null, null, null);
        var context = new ValidationContext<ProducerRow>(row);
        context.RootContextData[FeatureFlags.EnableLargeProducerEnhancedRecyclabilityRatingValidation] = true;

        var validator = new MaterialSubMaterialCombinationValidator();
        var result = validator.TestValidate(context);

        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
              .WithErrorCode(expectedError);
    }

    private static ProducerRow BuildProducerRow(string materialType, string? materialSubType)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, null, materialType, materialSubType, null, null, null, null, null, null);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string producerType, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, producerType, producerSize, packagingType, packagingClass, materialType, materialSubType, null, null, null, null, null, null, recyclabilityRating);
    }
}