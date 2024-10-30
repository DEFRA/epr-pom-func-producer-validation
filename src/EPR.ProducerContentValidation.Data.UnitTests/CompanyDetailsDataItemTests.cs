using EPR.ProducerContentValidation.Data.Models.CompanyDetailsApi;
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
            Assert.IsTrue(json.Contains("\"RN\":\"REF123\""), "Serialized JSON should contain 'RN' with correct value.");
            Assert.IsTrue(json.Contains("\"CHN\":\"CHN456\""), "Serialized JSON should contain 'CHN' with correct value.");
        }

        [TestMethod]
        public void DeserializeCompanyDetailsDataItem_FromJson_HasCorrectPropertyValues()
        {
            // Arrange
            var json = "{\"RN\":\"REF123\", \"CHN\":\"CHN456\"}";

            // Act
            var companyDetails = JsonConvert.DeserializeObject<CompanyDetailsDataItem>(json);

            // Assert
            Assert.IsNotNull(companyDetails, "Deserialized object should not be null.");
            Assert.AreEqual("REF123", companyDetails.ReferenceNumber, "ReferenceNumber should match the JSON value.");
            Assert.AreEqual("CHN456", companyDetails.CompaniesHouseNumber, "CompaniesHouseNumber should match the JSON value.");
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
            Assert.IsTrue(json.Contains("\"RN\":null"), "Serialized JSON should contain 'RN' with a null value.");
            Assert.IsTrue(json.Contains("\"CHN\":null"), "Serialized JSON should contain 'CHN' with a null value.");
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
            Assert.IsNotNull(companyDetailsWithNulls, "Deserialized object with nulls should not be null.");
            Assert.IsNull(companyDetailsWithNulls.ReferenceNumber, "ReferenceNumber should be null if JSON contains null.");
            Assert.IsNull(companyDetailsWithNulls.CompaniesHouseNumber, "CompaniesHouseNumber should be null if JSON contains null.");

            Assert.IsNotNull(companyDetailsWithoutKeys, "Deserialized object without keys should not be null.");
            Assert.IsNull(companyDetailsWithoutKeys.ReferenceNumber, "ReferenceNumber should be null if JSON key is missing.");
            Assert.IsNull(companyDetailsWithoutKeys.CompaniesHouseNumber, "CompaniesHouseNumber should be null if JSON key is missing.");
        }
    }
}
