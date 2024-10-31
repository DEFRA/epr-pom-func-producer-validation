using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;
using EPR.ProducerContentValidation.Application.Services.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.ProducerContentValidation.Application.UnitTests.Services.Helpers
{
    [TestClass]
    public class ProducerValidationEventIssueRequestMergerTests
    {
        [TestMethod]
        public void MergeRequests_IdenticalEntries_ShouldMergeErrorCodes()
        {
            // Arrange
            var request1 = new ProducerValidationEventIssueRequest(
                "123",
                "2024-01",
                1,
                "Producer1",
                "TypeA",
                "Large",
                "WasteType",
                "CategoryA",
                "MaterialX",
                "SubTypeX",
                "Nation1",
                "Nation2",
                "100",
                "Units",
                ErrorCodes: new List<string> { "Error1" });
            var request2 = new ProducerValidationEventIssueRequest(
                "123",
                "2024-01",
                1,
                "Producer1",
                "TypeA",
                "Large",
                "WasteType",
                "CategoryA",
                "MaterialX",
                "SubTypeX",
                "Nation1",
                "Nation2",
                "100",
                "Units",
                ErrorCodes: new List<string> { "Error2" });

            var list1 = new List<ProducerValidationEventIssueRequest> { request1 };
            var list2 = new List<ProducerValidationEventIssueRequest> { request2 };

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().HaveCount(1);
            result[0].ErrorCodes.Should().BeEquivalentTo(new List<string> { "Error1", "Error2" });
        }

        [TestMethod]
        public void MergeRequests_UniqueEntries_ShouldRetainBothEntries()
        {
            // Arrange
            var request1 = new ProducerValidationEventIssueRequest(
                "123",
                "2024-01",
                1,
                "Producer1",
                "TypeA",
                "Large",
                "WasteType",
                "CategoryA",
                "MaterialX",
                "SubTypeX",
                "Nation1",
                "Nation2",
                "100",
                "Units",
                ErrorCodes: new List<string> { "Error1" });
            var request2 = new ProducerValidationEventIssueRequest(
                "456",
                "2024-02",
                2,
                "Producer2",
                "TypeB",
                "Medium",
                "WasteType",
                "CategoryB",
                "MaterialY",
                "SubTypeY",
                "Nation3",
                "Nation4",
                "200",
                "Units",
                ErrorCodes: new List<string> { "Error3" });

            var list1 = new List<ProducerValidationEventIssueRequest> { request1 };
            var list2 = new List<ProducerValidationEventIssueRequest> { request2 };

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainEquivalentOf(request1);
            result.Should().ContainEquivalentOf(request2);
        }

        [TestMethod]
        public void MergeRequests_MixedEntries_ShouldMergeIdenticalAndRetainUnique()
        {
            // Arrange
            var request1 = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "123",
                DataSubmissionPeriod: "2024-01",
                RowNumber: 1,
                ProducerId: "Producer1",
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "WasteType",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubTypeX",
                FromHomeNation: "Nation1",
                ToHomeNation: "Nation2",
                QuantityKg: "100",
                QuantityUnits: "Units",
                ErrorCodes: new List<string> { "Error1" });

            var request2 = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "123",
                DataSubmissionPeriod: "2024-01",
                RowNumber: 1,
                ProducerId: "Producer1",
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "WasteType",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubTypeX",
                FromHomeNation: "Nation1",
                ToHomeNation: "Nation2",
                QuantityKg: "100",
                QuantityUnits: "Units",
                ErrorCodes: new List<string> { "Error2" });

            var uniqueRequest = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "789",
                DataSubmissionPeriod: "2024-03",
                RowNumber: 3,
                ProducerId: "Producer3",
                ProducerType: "TypeC",
                ProducerSize: "Small",
                WasteType: "WasteType",
                PackagingCategory: "CategoryC",
                MaterialType: "MaterialZ",
                MaterialSubType: "SubTypeZ",
                FromHomeNation: "Nation5",
                ToHomeNation: "Nation6",
                QuantityKg: "300",
                QuantityUnits: "Units",
                ErrorCodes: new List<string> { "Error4" });

            var list1 = new List<ProducerValidationEventIssueRequest> { request1, uniqueRequest };
            var list2 = new List<ProducerValidationEventIssueRequest> { request2 };

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(r => r.SubsidiaryId == "123" && r.ErrorCodes.Contains("Error1") && r.ErrorCodes.Contains("Error2"));
            result.Should().ContainEquivalentOf(uniqueRequest);
        }

        [TestMethod]
        public void MergeRequests_EmptyList_ShouldReturnOtherList()
        {
            // Arrange
            var request = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "123",
                DataSubmissionPeriod: "2024-01",
                RowNumber: 1,
                ProducerId: "Producer1",
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "WasteType",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubTypeX",
                FromHomeNation: "Nation1",
                ToHomeNation: "Nation2",
                QuantityKg: "100",
                QuantityUnits: "Units",
                ErrorCodes: new List<string> { "Error1" });

            var list1 = new List<ProducerValidationEventIssueRequest> { request };
            var list2 = new List<ProducerValidationEventIssueRequest>();

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainEquivalentOf(request);
        }

        [TestMethod]
        public void MergeRequests_BothEmpty_ShouldReturnEmpty()
        {
            // Arrange
            var list1 = new List<ProducerValidationEventIssueRequest>();
            var list2 = new List<ProducerValidationEventIssueRequest>();

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void MergeRequests_NullErrorCodes_ShouldHandleGracefully()
        {
            // Arrange
            var request1 = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "123",
                DataSubmissionPeriod: "2024-01",
                RowNumber: 1,
                ProducerId: "Producer1",
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "WasteType",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubTypeX",
                FromHomeNation: "Nation1",
                ToHomeNation: "Nation2",
                QuantityKg: "100",
                QuantityUnits: "Units",
                ErrorCodes: null);

            var request2 = new ProducerValidationEventIssueRequest(
                SubsidiaryId: "123",
                DataSubmissionPeriod: "2024-01",
                RowNumber: 1,
                ProducerId: "Producer1",
                ProducerType: "TypeA",
                ProducerSize: "Large",
                WasteType: "WasteType",
                PackagingCategory: "CategoryA",
                MaterialType: "MaterialX",
                MaterialSubType: "SubTypeX",
                FromHomeNation: "Nation1",
                ToHomeNation: "Nation2",
                QuantityKg: "100",
                QuantityUnits: "Units",
                ErrorCodes: new List<string> { "Error2" });

            var list1 = new List<ProducerValidationEventIssueRequest> { request1 };
            var list2 = new List<ProducerValidationEventIssueRequest> { request2 };

            // Act
            var result = ProducerValidationEventIssueRequestMerger.MergeRequests(list1, list2);

            // Assert
            result.Should().HaveCount(1);
            result[0].ErrorCodes.Should().BeEquivalentTo(new List<string> { "Error2" });
        }
    }
}
