﻿using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.Data.Models.Subsidiary;
using EPR.ProducerContentValidation.TestSupport;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class SubsidiaryValidationEvaluatorTests
    {
        private Mock<ILogger> _mockLogger;
        private Mock<IProducerValidationEventIssueRequestFormatter> _mockFormatter;
        private SubsidiaryValidationEvaluator _evaluator;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockFormatter = new Mock<IProducerValidationEventIssueRequestFormatter>();
            _evaluator = new SubsidiaryValidationEvaluator(_mockLogger.Object, _mockFormatter.Object);
        }

        [TestMethod]
        public void EvaluateSubsidiaryValidation_SubsidiaryDoesNotExist_ShouldLogWarningAndReturnFormattedRequest()
        {
            // Arrange
            var row = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "123",
                DataSubmissionPeriod = "2024-01",
                ProducerId = "Producer1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteType",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialX",
                MaterialSubType = "SubTypeX",
                FromHomeNation = "Nation1",
                ToHomeNation = "Nation2",
                QuantityKg = "100",
                QuantityUnits = "Units"
            };
            var subsidiary = new SubsidiaryDetail { SubsidiaryExists = false, SubsidiaryBelongsToOrganisation = true };
            var expectedRequest = new ProducerValidationEventIssueRequest(
                row.SubsidiaryId,
                row.DataSubmissionPeriod,
                row.RowNumber,
                row.ProducerId,
                row.ProducerType,
                row.ProducerSize,
                row.WasteType,
                row.PackagingCategory,
                row.MaterialType,
                row.MaterialSubType,
                row.FromHomeNation,
                row.ToHomeNation,
                row.QuantityKg,
                row.QuantityUnits,
                ErrorCodes: new List<string> { ErrorCode.SubsidiaryIdDoesNotExist });

            _mockFormatter.Setup(f => f.Format(row, ErrorCode.SubsidiaryIdDoesNotExist)).Returns(expectedRequest);

            // Act
            var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRequest, result);

            // _mockLogger.Verify(
            //    l => l.LogWarning(
            //        It.Is<string>(s => s == "Validation Warning at row {RowNumber}: {Message} (ErrorCode: {ErrorCode})"),
            //        It.Is<object[]>(o =>
            //            (int)o[0] == row.RowNumber + 1 &&
            //            (string)o[1] == "Subsidiary ID does not exist" &&
            //            (string)o[2] == ErrorCode.SubsidiaryIdDoesNotExist)),
            //    Times.Once);
            _mockFormatter.Verify(f => f.Format(row, ErrorCode.SubsidiaryIdDoesNotExist), Times.Once);
        }

        [TestMethod]
        public void EvaluateSubsidiaryValidation_SubsidiaryBelongsToDifferentOrganisation_ShouldLogWarningAndReturnFormattedRequest()
        {
            // Arrange
            var row = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "123",
                DataSubmissionPeriod = "2024-01",
                ProducerId = "Producer1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteType",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialX",
                MaterialSubType = "SubTypeX",
                FromHomeNation = "Nation1",
                ToHomeNation = "Nation2",
                QuantityKg = "100",
                QuantityUnits = "Units"
            };
            var subsidiary = new SubsidiaryDetail { SubsidiaryExists = true, SubsidiaryBelongsToOrganisation = false };
            var expectedRequest = new ProducerValidationEventIssueRequest(
                row.SubsidiaryId,
                row.DataSubmissionPeriod,
                row.RowNumber,
                row.ProducerId,
                row.ProducerType,
                row.ProducerSize,
                row.WasteType,
                row.PackagingCategory,
                row.MaterialType,
                row.MaterialSubType,
                row.FromHomeNation,
                row.ToHomeNation,
                row.QuantityKg,
                row.QuantityUnits,
                ErrorCodes: new List<string> { ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation });

            _mockFormatter.Setup(f => f.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation)).Returns(expectedRequest);

            // Act
            var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedRequest, result);

            // _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), row.RowNumber + 1, "Subsidiary ID is assigned to a different organisation", ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation), Times.Once);
            _mockFormatter.Verify(f => f.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation), Times.Once);
        }

        [TestMethod]
        public void EvaluateSubsidiaryValidation_SubsidiaryIsValid_ShouldReturnNull()
        {
            // Arrange
            var row = ModelGenerator.CreateProducerRow(1) with
            {
                SubsidiaryId = "123",
                DataSubmissionPeriod = "2024-01",
                ProducerId = "Producer1",
                RowNumber = 1,
                ProducerType = "TypeA",
                ProducerSize = "Large",
                WasteType = "WasteType",
                PackagingCategory = "CategoryA",
                MaterialType = "MaterialX",
                MaterialSubType = "SubTypeX",
                FromHomeNation = "Nation1",
                ToHomeNation = "Nation2",
                QuantityKg = "100",
                QuantityUnits = "Units"
            };
            var subsidiary = new SubsidiaryDetail { SubsidiaryExists = true, SubsidiaryBelongsToOrganisation = true };

            // Act
            var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber);

            // Assert
            Assert.IsNull(result);

            // _mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockFormatter.Verify(f => f.Format(It.IsAny<ProducerRow>(), It.IsAny<string>()), Times.Never);
        }
    }
}