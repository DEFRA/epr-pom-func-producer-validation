# Integration Tests – Validate Producer Content

Integration tests for the **validate-producer-content** HTTP endpoint. They send real HTTP requests to the function app and assert on validation errors and warnings.

## Prerequisites

1. **Function app running locally** with the HTTP trigger enabled:
   - Run the function app (e.g. from `EPR.ProducerContentValidation.FunctionApp`).
   - In `settings.json` (or `local.settings.json`), set:
     - `HttpEndpoint:Enabled` = `true`
   - Default URL: `http://localhost:7071`.

2. **Submission period configuration**  
   Some tests expect valid data submission periods (e.g. `2026-P1`, `January to June 2026`). Ensure the function app’s `SubmissionPeriods` configuration includes the periods used in the tests, or those tests may report different errors (e.g. 44).

3. **Feature flags (for full coverage)**  
   Tests assume the following feature flags are **enabled** in the function app (`local.settings.json` or `settings.json`):
   - `FeatureManagement:EnableLargeProducerRecyclabilityRatingValidation` = `true` — recyclability tests (102, 106, 108, 109)
   - `FeatureManagement:EnableLargeProducerEnhancedRecyclabilityRatingValidation` = `true` — enhanced recyclability (108, 109)
   
   With these set, all feature-flagged API tests will pass.

## Running the integration tests

The integration test project lives under **`src/`**. To run **only** the integration tests (no unit or other test projects), target the integration test project directly:

**From the repository root** (recommended):


First start the function app using docker in a separate terminal:

```bash
docker compose up --build
```

Then run the tests:

```bash
dotnet test src/EPR.ProducerContentValidation.IntegrationTests/EPR.ProducerContentValidation.IntegrationTests.csproj --filter "Category=IntegrationTest"
```

**From the `src` directory**:

```bash
cd src
dotnet test EPR.ProducerContentValidation.IntegrationTests/EPR.ProducerContentValidation.IntegrationTests.csproj --filter "Category=IntegrationTest"
```

**Via the solution** (builds and runs all projects; only tests matching the filter run; other test projects still build and report “no match”):

```bash
dotnet test src/EPR.ProducerContentValidation.sln --filter "Category=IntegrationTest"
```

Override the base URL if the function runs on a different host/port:

```bash
# From repo root
VALIDATE_PRODUCER_CONTENT_BASE_URL=http://localhost:7071 dotnet test src/EPR.ProducerContentValidation.IntegrationTests/EPR.ProducerContentValidation.IntegrationTests.csproj --filter "Category=IntegrationTest"
```

## Debugging failed tests (expected vs actual)

Every integration test run writes the **actual** API result to test output (status code, error codes, warning codes, and full response JSON). When a test fails:

1. Open the test result and click **Output** (or view the test output in your runner).
2. Find the section **`=== API result (actual – use when debugging failures) ===`**.
3. Compare **Error codes (actual)** and **Full response (JSON)** with what your test expected.

No environment variable is required; this is written for every call to `ValidateAndLogAsync`.

## Viewing request payloads (for Postman)

To see the exact JSON request body used in each test so you can copy it into Postman:

1. Set the environment variable **`OUTPUT_POSTMAN_PAYLOADS=1`** (or `true`).
2. Run the tests (e.g. a single test or the whole project).
3. In the test results, open **Output** for the test; you’ll see the URL, method, and the request JSON between the `===` lines. Copy the JSON into Postman’s request body.

Example:

```bash
OUTPUT_POSTMAN_PAYLOADS=1 dotnet test src/EPR.ProducerContentValidation.IntegrationTests/EPR.ProducerContentValidation.IntegrationTests.csproj --filter "Category=IntegrationTest&FullyQualifiedName~Invalid_producer_id_returns_error_01"
```

In Visual Studio or VS Code Test Explorer, run a test with the variable set and use the test’s **Output** link to view the payload.

## Structure

| Area | File | Error / warning codes covered |
|------|------|-------------------------------|
| Core fields | `CoreFieldValidationApiTests.cs` | 01, 02, 03, 04, 05, 07, 08, 09, 10, 13, 41, 44, 46, 90, 91 |
| Packaging / waste type | `PackagingCategoryAndWasteTypeApiTests.cs` | 22, 23, 25, 33, 42, 43, 53 |
| Recyclability / material subtype | `RecyclabilityAndMaterialSubTypeApiTests.cs` | 45, 103, 106, 107 |
| Duplicates / period consistency | `DuplicateAndDataSubmissionPeriodApiTests.cs` | 40, 50 |
| Warnings and edge cases | `WarningsAndEdgeCasesApiTests.cs` | 59, 60, 62, empty body, invalid JSON |

- **`ValidateProducerContentApiClient`** – HTTP client that POSTs to `/api/validate-producer-content?skipApiCall=true` and returns a result with error/warning helpers.
- **`ValidateProducerContentRequestBuilder`** – Builds valid and invalid request payloads (e.g. `ValidRequest()`, `ValidRow()`, `RequestWithDuplicateRows()`).
- **`ValidateProducerContentApiFixture`** – xUnit fixture that provides the client and base URL (from env or default `http://localhost:7071`).

## Adding tests for new error codes

1. In **`ValidateProducerContentRequestBuilder`**, add (or reuse) a helper that builds a row/request that triggers the desired rule.
2. In the appropriate test class (or a new one), add a test that:
   - Builds the request (e.g. `ValidRequest()` with an overridden row).
   - Calls `_client.ValidateAsync(request)`.
   - Asserts `result.HasErrorCode(ErrorCode.YourErrorCode)` or `result.HasWarningCode(...)`.

All error code constants are in `EPR.ProducerContentValidation.Application.Constants.ErrorCode`.
