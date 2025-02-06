namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;
using Moq;

[TestClass]
public class LargeProducerRecyclabilityRatingValidatorTests : LargeProducerRecyclabilityRatingValidator
{
    private readonly LargeProducerRecyclabilityRatingValidator _systemUnderTest;
    private Mock<IFeatureManager> _featureManagerMock;

    public LargeProducerRecyclabilityRatingValidatorTests()
    {
        _systemUnderTest = new LargeProducerRecyclabilityRatingValidator();
        _featureManagerMock = new Mock<IFeatureManager>();
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", RecyclabilityRating.Green)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Other, MaterialSubType.PET, RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Amber)]
    [DataRow(DataSubmissionPeriod.Year2025H2, ProducerSize.Large, PackagingType.Household, PackagingClass.ShipmentPackaging, MaterialType.Plastic, MaterialSubType.Flexible, RecyclabilityRating.GreenMedical)]
    public void LargeProducerRecyclabilityRatingValidator_Recyclability_Code_ValidSubmission(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RecyclabilityRating);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, "", RecyclabilityRating.Red)]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Plastic_Material_SubType(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
             .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Red)]
    public void LargeProducerRecyclabilityRatingValidator_Plastic_Material_SubType_Not_Required_Before_2025(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
        .WithErrorCode(ErrorCode.PackagingMaterialSubtypeNotNeededForPackagingMaterial);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.HDPE, RecyclabilityRating.Red)]
    public void LargeProducerRecyclabilityRatingValidator_Invalid_Plastic_Material_SubType(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaterialSubType)
            .WithErrorCode(ErrorCode.LargeProducerPlasticMaterialSubTypeInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", "K-D")]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, "Z")]
    public void LargeProducerRecyclabilityRatingValidator_Invalid_Recyclability_Code(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
            .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingInvalidErrorCode);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", "")]
    [DataRow(DataSubmissionPeriod.Year2025H1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, "")]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingRequired);
    }

    [TestMethod]
    [DataRow(DataSubmissionPeriod.Year2023P1, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Aluminium, "", RecyclabilityRating.Red)]
    [DataRow(DataSubmissionPeriod.Year2023P3, ProducerSize.Large, PackagingType.Household, PackagingClass.PrimaryPackaging, MaterialType.Plastic, MaterialSubType.Rigid, RecyclabilityRating.Green)]
    public void LargeProducerRecyclabilityRatingValidator_Missing_Recyclability_Code_Not_Required_Before_2025(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        // Arrange
        var producerRow = BuildProducerRow(dataSubmissionPeriod, producerSize, packagingType, packagingClass, materialType, materialSubType, recyclabilityRating);

        // Act
        var result = _systemUnderTest.TestValidate(producerRow);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RecyclabilityRating)
       .WithErrorCode(ErrorCode.LargeProducerRecyclabilityRatingNotRequired);
    }

    private static ProducerRow BuildProducerRow(string producerSize)
    {
        return new ProducerRow(null, null, null, 1, null, producerSize, null, null, null, null, null, null, null, null, null, null, null);
    }

    private static ProducerRow BuildProducerRow(string dataSubmissionPeriod, string producerSize, string packagingType, string packagingClass, string materialType, string materialSubType, string recyclabilityRating)
    {
        return new ProducerRow(null, dataSubmissionPeriod, null, 1, null, producerSize, packagingType, packagingClass, materialType, materialSubType, null, null, null, null, null, recyclabilityRating, null);
    }
}