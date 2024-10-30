using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class ProducerValidationEventFormatterTests
    {
        private readonly IProducerValidationEventIssueRequestFormatter _formatter;

        public ProducerValidationEventFormatterTests()
        {
            _formatter = new ProducerValidationEventIssueRequestFormatter();
        }

        [TestMethod]
        public void Format_ShouldReturnFormattedRequest_WithAllFieldsProvided()
        {
            // Arrange
            var row = new ProducerRow(
                SubsidiaryId: "Sub123",
                DataSubmissionPeriod: "2023-Q1",
                ProducerId: "Org456",
                RowNumber: 1,
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "Plastic",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubMaterialX",
                FromHomeNation: "UK",
                ToHomeNation: "Germany",
                QuantityKg: "100",
                QuantityUnits: "200",
                SubmissionPeriod: "2023");

            string errorCode = "ErrorCode123";

            // Act
            var result = _formatter.Format(row, errorCode);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Sub123", result.SubsidiaryId);
            Assert.AreEqual("2023-Q1", result.DataSubmissionPeriod);
            Assert.AreEqual(1, result.RowNumber);
            Assert.AreEqual("Org456", result.ProducerId);
            Assert.AreEqual("TypeA", result.ProducerType);
            Assert.AreEqual("Large", result.ProducerSize);
            Assert.AreEqual("Plastic", result.WasteType);
            Assert.AreEqual("CategoryA", result.PackagingCategory);
            Assert.AreEqual("MaterialX", result.MaterialType);
            Assert.AreEqual("SubMaterialX", result.MaterialSubType);
            Assert.AreEqual("UK", result.FromHomeNation);
            Assert.AreEqual("Germany", result.ToHomeNation);
            Assert.AreEqual("100", result.QuantityKg);
            Assert.AreEqual("200", result.QuantityUnits);
            CollectionAssert.Contains(result.ErrorCodes, errorCode);
        }

        [TestMethod]
        public void Format_ShouldHandleNullFields_InProducerRow()
        {
            // Arrange
            var row = new ProducerRow(
                SubsidiaryId: null,
                DataSubmissionPeriod: null,
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
                SubmissionPeriod: "2023");

            string errorCode = "ErrorCode123";

            // Act
            var result = _formatter.Format(row, errorCode);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.SubsidiaryId);
            Assert.AreEqual(string.Empty, result.DataSubmissionPeriod);
            Assert.AreEqual(1, result.RowNumber);
            Assert.AreEqual(string.Empty, result.ProducerId);
            Assert.AreEqual(string.Empty, result.ProducerType);
            Assert.AreEqual(string.Empty, result.ProducerSize);
            Assert.AreEqual(string.Empty, result.WasteType);
            Assert.AreEqual(string.Empty, result.PackagingCategory);
            Assert.AreEqual(string.Empty, result.MaterialType);
            Assert.AreEqual(string.Empty, result.MaterialSubType);
            Assert.AreEqual(string.Empty, result.FromHomeNation);
            Assert.AreEqual(string.Empty, result.ToHomeNation);
            Assert.AreEqual(string.Empty, result.QuantityKg);
            Assert.AreEqual(string.Empty, result.QuantityUnits);
            CollectionAssert.Contains(result.ErrorCodes, errorCode);
        }

        [TestMethod]
        public void Format_ShouldReturnSingleErrorCode()
        {
            // Arrange
            var row = new ProducerRow(
                SubsidiaryId: "Sub123",
                DataSubmissionPeriod: "2023-Q1",
                ProducerId: "Org456",
                RowNumber: 1,
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "Plastic",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubMaterialX",
                FromHomeNation: "UK",
                ToHomeNation: "Germany",
                QuantityKg: "100",
                QuantityUnits: "200",
                SubmissionPeriod: "2023");

            string errorCode = "SingleError";

            // Act
            var result = _formatter.Format(row, errorCode);

            // Assert
            Assert.AreEqual(1, result.ErrorCodes.Count, "Expected exactly one error code in the list.");
            Assert.AreEqual("SingleError", result.ErrorCodes[0]);
        }
    }
}
