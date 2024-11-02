using EPR.ProducerContentValidation.Application.DTOs.SubmissionApi;

namespace EPR.ProducerContentValidation.Application.Services.Helpers;

public static class ProducerValidationEventIssueRequestMerger
{
    public static List<ProducerValidationEventIssueRequest> MergeRequests(
        List<ProducerValidationEventIssueRequest> list1,
        List<ProducerValidationEventIssueRequest> list2)
    {
        // Combine both lists
        var combinedList = list1.Concat(list2);

        // Use a dictionary to group and merge ErrorCodes for identical records
        var mergedDict = new Dictionary<(
            string SubsidiaryId,
            string DataSubmissionPeriod,
            int RowNumber,
            string ProducerId,
            string ProducerType,
            string ProducerSize,
            string WasteType,
            string PackagingCategory,
            string MaterialType,
            string MaterialSubType,
            string FromHomeNation,
            string ToHomeNation,
            string QuantityKg,
            string QuantityUnits,
            string BlobName),
            ProducerValidationEventIssueRequest>();

        foreach (var request in combinedList)
        {
            var key = (
                request.SubsidiaryId,
                request.DataSubmissionPeriod,
                request.RowNumber,
                request.ProducerId,
                request.ProducerType,
                request.ProducerSize,
                request.WasteType,
                request.PackagingCategory,
                request.MaterialType,
                request.MaterialSubType,
                request.FromHomeNation,
                request.ToHomeNation,
                request.QuantityKg,
                request.QuantityUnits,
                request.BlobName);

            if (mergedDict.TryGetValue(key, out var existingRequest))
            {
                // Merge ErrorCodes if they exist
                var mergedErrorCodes = existingRequest.ErrorCodes?.Union(request.ErrorCodes ?? new List<string>()).ToList()
                                    ?? request.ErrorCodes ?? new List<string>();

                // Update the dictionary with the merged ErrorCodes
                mergedDict[key] = existingRequest with { ErrorCodes = mergedErrorCodes };
            }
            else
            {
                // Add the request to the dictionary if it doesn't exist
                mergedDict[key] = request;
            }
        }

        // Return the merged list
        return mergedDict.Values.ToList();
    }
}
