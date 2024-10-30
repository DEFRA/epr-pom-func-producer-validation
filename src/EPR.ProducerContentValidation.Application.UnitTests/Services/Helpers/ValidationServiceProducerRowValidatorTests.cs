using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using EPR.ProducerContentValidation.TestSupport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class ValidationServiceProducerRowValidatorTests
    {
        private Mock<IFindMatchingProducer> _mockFindMatchingProducer;
        private ValidationServiceProducerRowValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _mockFindMatchingProducer = new Mock<IFindMatchingProducer>();
            _validator = new ValidationServiceProducerRowValidator(_mockFindMatchingProducer.Object);
        }

        [TestMethod]
        public void ProcessRowsForValidationErrors_NoValidationErrors_ReturnsEmptyList()
        {
            // Arrange
            var producerRow = ModelGenerator.CreateProducerRow(1);
            var producerRowTwo = ModelGenerator.CreateProducerRow(2);
            var producerRows = new List<ProducerRow> { producerRow, producerRowTwo };

            var row1 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub1",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteTypeA",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialA",
                MaterialSubType = "SubTypeA",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "100",
                QuantityUnits = "10"
            };

            var row2 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub2",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod2",
                RowNumber = 2,
                ProducerType = "TypeB",
                ProducerSize = "Small",
                WasteType = "WasteTypeB",
                PackagingCategory = "CategoryB",
                MaterialType = "MaterialB",
                MaterialSubType = "SubTypeB",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "200",
                QuantityUnits = "20"
            };
            var rows = new List<ProducerRow>
            {
                row1,
                row2
            };

            var response = new SubsidiaryDetailsResponse
            {
                // Initialize your response data
            };

            _mockFindMatchingProducer.Setup(x => x.Match(It.IsAny<ProducerRow>(), response, It.IsAny<int>())).Returns((ProducerValidationEventIssueRequest)null);

            // Act
            var result = _validator.ProcessRowsForValidationErrors(rows, response);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Any());
        }

        [TestMethod]
        public void ProcessRowsForValidationErrors_SingleValidationError_ReturnsListWithOneError()
        {
            // Arrange
            var row1 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub1",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteTypeA",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialA",
                MaterialSubType = "SubTypeA",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "100",
                QuantityUnits = "10"
            };

            var row2 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub2",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod2",
                RowNumber = 2,
                ProducerType = "TypeB",
                ProducerSize = "Small",
                WasteType = "WasteTypeB",
                PackagingCategory = "CategoryB",
                MaterialType = "MaterialB",
                MaterialSubType = "SubTypeB",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "200",
                QuantityUnits = "20"
            };
            var rows = new List<ProducerRow>
            {
                row1,
                row2
            };

            var response = new SubsidiaryDetailsResponse
            {
                // Initialize your response data
            };

            var errorRequest = new ProducerValidationEventIssueRequest("Sub1", "2024Q1", 1, "Prod1", "TypeA", "Large", "WasteTypeA", "CategoryA", "MaterialA", "SubTypeA", "NationA", "NationB", "100", "10", ErrorCodes: new List<string> { "Error1" });

            _mockFindMatchingProducer.Setup(x => x.Match(rows[0], response, 0)).Returns(errorRequest);
            _mockFindMatchingProducer.Setup(x => x.Match(rows[1], response, 1)).Returns((ProducerValidationEventIssueRequest)null);

            // Act
            var result = _validator.ProcessRowsForValidationErrors(rows, response);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(errorRequest, result.First());
        }

        [TestMethod]
        public void ProcessRowsForValidationErrors_MultipleValidationErrors_ReturnsListWithAllErrors()
        {
            // Arrange
            var row1 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub1",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteTypeA",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialA",
                MaterialSubType = "SubTypeA",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "100",
                QuantityUnits = "10"
            };

            var row2 = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "Sub2",
                DataSubmissionPeriod = "2024Q1",
                ProducerId = "Prod2",
                RowNumber = 2,
                ProducerType = "TypeB",
                ProducerSize = "Small",
                WasteType = "WasteTypeB",
                PackagingCategory = "CategoryB",
                MaterialType = "MaterialB",
                MaterialSubType = "SubTypeB",
                FromHomeNation = "NationA",
                ToHomeNation = "NationB",
                QuantityKg = "200",
                QuantityUnits = "20"
            };
            var rows = new List<ProducerRow>
            {
                row1,
                row2
            };

            var response = new SubsidiaryDetailsResponse
            {
                // Initialize your response data
            };

            var errorRequest1 = new ProducerValidationEventIssueRequest("Sub1", "2024Q1", 1, "Prod1", "TypeA", "Large", "WasteTypeA", "CategoryA", "MaterialA", "SubTypeA", "NationA", "NationB", "100", "10", ErrorCodes: new List<string> { "Error1" });
            var errorRequest2 = new ProducerValidationEventIssueRequest("Sub2", "2024Q1", 2, "Prod2", "TypeB", "Small", "WasteTypeB", "CategoryB", "MaterialB", "SubTypeB", "NationA", "NationB", "200", "20", ErrorCodes: new List<string> { "Error2" });

            _mockFindMatchingProducer.Setup(x => x.Match(rows[0], response, 0)).Returns(errorRequest1);
            _mockFindMatchingProducer.Setup(x => x.Match(rows[1], response, 1)).Returns(errorRequest2);

            // Act
            var result = _validator.ProcessRowsForValidationErrors(rows, response);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Contains(errorRequest1));
            Assert.IsTrue(result.Contains(errorRequest2));
        }
    }
}
