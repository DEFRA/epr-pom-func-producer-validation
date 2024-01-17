namespace EPR.ProducerContentValidation.Application.Constants;

public static class StoreKey
{
    public static string FetchStoreKey(string blobName, string issueType)
    {
        return $"{blobName}:{issueType}";
    }
}