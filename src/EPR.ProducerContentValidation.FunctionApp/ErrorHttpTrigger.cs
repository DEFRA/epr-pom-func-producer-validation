namespace EPR.EventDispatcher.Functions
{
    using System.Net;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;

    /// <summary>
    /// Health check endpoint "api/error".
    /// "/api/error" will return 500 Internal Serer Error.
    /// </summary>
    public static class ErrorHttpTrigger
    {
        [FunctionName(nameof(ErrorHttpTrigger))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "error")] HttpRequest req)
        {
            return new OkObjectResult("Error") { StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}