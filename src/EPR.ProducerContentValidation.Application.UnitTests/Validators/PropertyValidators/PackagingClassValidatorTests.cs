namespace EPR.ProducerContentValidation.Application.UnitTests.Validators.PropertyValidators;

using Application.Validators.PropertyValidators;
using Constants;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Models;

[TestClass]
public class PackagingClassValidatorTests
{
    private PackagingClassValidator _systemUnderTest;

    [TestInitialize]
    public void Initialize()
    {
        _systemUnderTest = new PackagingClassValidator();
    }

    [TestMethod]
    [DataRow(PackagingClass.PrimaryPackaging)]
    [DataRow(PackagingClass.SecondaryPackaging)]
    [DataRow(PackagingClass.ShipmentPackaging)]
    [DataRow(PackagingClass.TransitPackaging)]
    [DataRow(PackagingClass.NonPrimaryPackaging)]
    [DataRow(PackagingClass.TotalPackaging)]
    [DataRow(PackagingClass.TotalRelevantWaste)]
    [DataRow(PackagingClass.WasteOrigin)]
    [DataRow(PackagingClass.PublicBin)]
    [DataRow(null)]
    public void PackagingClassValidator_PassesValidation_WhenPackagingClassIs(string packagingClass)
    {
        // Arrange
        var model = BuildProducerRow(packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PackagingCategory);
    }

    [TestMethod]
    [DataRow("XX")]
    [DataRow("S1")]
    [DataRow("S2")]
    [DataRow("")]
    [DataRow("O3")]
    public void PackagingClassValidator_FailsValidation_WhenPackagingClassIsInvalid(string packagingClass)
    {
        // Arrange
        var model = BuildProducerRow(packagingClass);

        // Act
        var result = _systemUnderTest.TestValidate(model);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.PackagingCategory)
            .WithErrorCode(ErrorCode.PackagingCategoryInvalidErrorCode);
    }

    private static ProducerRow BuildProducerRow(string packagingClass)
    {
        return new ProducerRow(null, null, null, 1, null, null, null, packagingClass, null, null, null, null, null, null, null);
    }
}