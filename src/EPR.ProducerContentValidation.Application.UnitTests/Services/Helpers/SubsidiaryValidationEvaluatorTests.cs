﻿using EPR.ProducerContentValidation.Application.Constants;
using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Models.Subsidiary;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using EPR.ProducerContentValidation.Application.Services.Helpers.Interfaces;
using EPR.ProducerContentValidation.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers;

[TestClass]
public class SubsidiaryValidationEvaluatorTests
{
    private Mock<ILogger<SubsidiaryValidationEvaluator>> _mockLogger;
    private Mock<IProducerValidationEventIssueRequestFormatter> _mockFormatter;
    private SubsidiaryValidationEvaluator _evaluator;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<SubsidiaryValidationEvaluator>>();
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
            QuantityUnits = "Units",
            TransitionalPackagingUnits = "50",
            RecyclabilityRating = "A"
        };
        var subsidiary = new SubsidiaryDetail { SubsidiaryExists = false, SubsidiaryBelongsToAnyOtherOrganisation = true };
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
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating,
            ErrorCodes: new List<string> { ErrorCode.SubsidiaryIdDoesNotExist });

        _mockFormatter.Setup(f => f.Format(row, ErrorCode.SubsidiaryIdDoesNotExist, It.IsAny<string>())).Returns(expectedRequest);
        string blobName = string.Empty;

        // Act
        var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber, blobName);

        // Assert
        result.Should().NotBeNull("because a request should be returned if the subsidiary does not exist.")
            .And.BeEquivalentTo(expectedRequest, "because the returned request should match the expected formatted request.");

        _mockFormatter.Verify(f => f.Format(row, ErrorCode.SubsidiaryIdDoesNotExist, It.IsAny<string>()), Times.Once);
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
            QuantityUnits = "Units",
            TransitionalPackagingUnits = "50",
            RecyclabilityRating = "A"
        };
        var subsidiary = new SubsidiaryDetail { SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = true };
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
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating,
            ErrorCodes: new List<string> { ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation });

        _mockFormatter.Setup(f => f.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation, It.IsAny<string>())).Returns(expectedRequest);
        string blobName = string.Empty;

        // Act
        var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber, blobName);

        // Assert
        result.Should().NotBeNull("because a request should be returned if the subsidiary belongs to a different organisation.")
            .And.BeEquivalentTo(expectedRequest, "because the returned request should match the expected formatted request.");

        _mockFormatter.Verify(f => f.Format(row, ErrorCode.SubsidiaryIdIsAssignedToADifferentOrganisation, It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public void EvaluateSubsidiaryValidation_SubsidiaryDoesNotBelongToAnyOrganisation_ShouldLogWarningAndReturnFormattedRequest()
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
            QuantityUnits = "Units",
            TransitionalPackagingUnits = "50",
            RecyclabilityRating = "A"
        };
        var subsidiary = new SubsidiaryDetail { SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false, SubsidiaryDoesNotBelongToAnyOrganisation = true };
        var expectedResponse = new ProducerValidationEventIssueRequest(
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
            row.TransitionalPackagingUnits,
            row.RecyclabilityRating,
            ErrorCodes: new List<string> { ErrorCode.SubsidiaryDoesNotBelongToAnyOrganisation });

        _mockFormatter.Setup(f => f.Format(row, ErrorCode.SubsidiaryDoesNotBelongToAnyOrganisation, It.IsAny<string>())).Returns(expectedResponse);
        string blobName = string.Empty;

        // Act
        var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber, blobName);

        // Assert
        result.Should().NotBeNull("because a request should be returned if the subsidiary belongs to no organisation.")
            .And.BeEquivalentTo(expectedResponse, "because the returned response should match the expected formatted response.");

        _mockFormatter.Verify(f => f.Format(row, ErrorCode.SubsidiaryDoesNotBelongToAnyOrganisation, It.IsAny<string>()), Times.Once);
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
            QuantityUnits = "Units",
            TransitionalPackagingUnits = "50",
            RecyclabilityRating = "A"
        };
        var subsidiary = new SubsidiaryDetail { SubsidiaryExists = true, SubsidiaryBelongsToAnyOtherOrganisation = false };
        string blobName = string.Empty;

        // Act
        var result = _evaluator.EvaluateSubsidiaryValidation(row, subsidiary, row.RowNumber, blobName);

        // Assert
        result.Should().BeNull("because the subsidiary is valid and should not generate any validation issue.");

        _mockFormatter.Verify(f => f.Format(It.IsAny<ProducerRow>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
