using EPR.ProducerContentValidation.Application.Models.CompanyDetailsApi;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace EPR.ProducerContentValidation.Data.UnitTests
{
    [TestClass]
    public class CompanyDetailsDataItemTests
    {
        [TestMethod]
        public void SerializeCompanyDetailsDataItem_ToJson_ContainsCorrectJsonPropertyNames()
        {
            // Arrange
            var companyDetails = new CompanyDetailsDataItem
            {
                ReferenceNumber = "REF123",
                CompaniesHouseNumber = "CHN456"
            };

            // Act
            var json = JsonConvert.SerializeObject(companyDetails);

            // Assert
            json.Should().Contain("\"RN\":\"REF123\"")
                .And.Contain("\"CHN\":\"CHN456\"");
        }

        [TestMethod]
        public void DeserializeCompanyDetailsDataItem_FromJson_HasCorrectPropertyValues()
        {
            // Arrange
            var json = "{\"RN\":\"REF123\", \"CHN\":\"CHN456\"}";

            // Act
            var companyDetails = JsonConvert.DeserializeObject<CompanyDetailsDataItem>(json);

            // Assert
            companyDetails.Should().NotBeNull();
            companyDetails!.ReferenceNumber.Should().Be("REF123");
            companyDetails.CompaniesHouseNumber.Should().Be("CHN456");
        }

        [TestMethod]
        public void SerializeCompanyDetailsDataItem_NullValues_HasNullJsonProperties()
        {
            // Arrange
            var companyDetails = new CompanyDetailsDataItem
            {
                ReferenceNumber = null,
                CompaniesHouseNumber = null
            };

            // Act
            var json = JsonConvert.SerializeObject(companyDetails);

            // Assert
            json.Should().Contain("\"RN\":null")
                .And.Contain("\"CHN\":null");
        }

        [TestMethod]
        public void DeserializeCompanyDetailsDataItem_NullOrMissingValues_PropertiesAreNull()
        {
            // Arrange
            var jsonWithNulls = "{\"RN\":null, \"CHN\":null}";
            var jsonWithoutKeys = "{}";

            // Act
            var companyDetailsWithNulls = JsonConvert.DeserializeObject<CompanyDetailsDataItem>(jsonWithNulls);
            var companyDetailsWithoutKeys = JsonConvert.DeserializeObject<CompanyDetailsDataItem>(jsonWithoutKeys);

            // Assert
            companyDetailsWithNulls.Should().NotBeNull();
            companyDetailsWithNulls!.ReferenceNumber.Should().BeNull();
            companyDetailsWithNulls.CompaniesHouseNumber.Should().BeNull();

            companyDetailsWithoutKeys.Should().NotBeNull();
            companyDetailsWithoutKeys!.ReferenceNumber.Should().BeNull();
            companyDetailsWithoutKeys.CompaniesHouseNumber.Should().BeNull();
        }
    }
}
