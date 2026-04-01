using Xunit;

namespace EPR.ProducerContentValidation.ApiTests;

/// <summary>
/// Collection definition so that <see cref="ValidateProducerContentApiFixture"/> is shared across all API test classes
/// (one probe per test run instead of per class).
/// </summary>
[CollectionDefinition("ValidateProducerContentApi")]
public class ValidateProducerContentApiCollection : ICollectionFixture<ValidateProducerContentApiFixture>
{
}
