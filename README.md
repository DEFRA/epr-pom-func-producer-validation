# EPR Producer Content Validation Function

## Overview

This function listens to an Azure ServiceBus Queue, for messages indicating a user has uploaded a POM file via the front end, to blob storage. It retreives the file, and vaidates its contents based on a set validation rules.

## Placed on Market file Specification
See [PoM file specification](https://www.gov.uk/government/publications/packaging-data-how-to-create-your-file-for-extended-producer-responsibility/packaging-data-file-specification-for-extended-producer-responsibility).

## Service Bus Payload
An example of payload passed into the Azure service bus can be seen below.
```json
{
  "organisationId": "536135e1-242b-49e8-9be0-ccee28d95bef",
  "userId": "536135e1-242b-49e8-9be0-ccee28d95bef",
  "userType": 1,
  "submissionId": "5988110e-d2b2-49f8-b35c-9e97e2f359b7",
  "producerId": "997778",
  "blobName": "5df31283-2bb7-4119-b09f-5b51e639bacb",
  "rows": [
    {
      "RowNumber": 1,
      "ProducerId": "997778",
      "SubsidiaryId": "1",
      "ProducerSize": "L",
      "DataSubmissionPeriod": "2023-P1",
      "SubmissionPeriod": "January to June 2023",
      "ProducerType": "SO",
      "MaterialSubType": null,
      "WasteType": "HDC",
      "PackagingCategory": null,
      "MaterialType": "PL",
      "FromHomeNation": null,
      "ToHomeNation": null,
      "QuantityKg": 1234,
      "QuantityUnits": 1000
    }
  ]
}
```

> **Note**
> The blob name will need to be a key set in Redis set to the value of '0'.
> This is done to keep track of the number of errors the validator finds in the file, to prevent it from processing more errors than the maximum allowed.

## How To Run

### Prerequisites
In order to run the service you will need the following dependencies:

- .NET 8
- Redis
- Azure CLI

### developer configuration

You will need a configuration for the periods being validated.
The configuration periods should be added to the Values collection of the local settings.
``` json
{
  "Values": {
    "SubmissionPeriods:0:SubmissionPeriod": "January to June 2023",
    "SubmissionPeriods:0:PeriodCodes:0": "2023-P1",
    "SubmissionPeriods:0:PeriodCodes:1": "2023-P2",
    "SubmissionPeriods:0:ErrorCode": "54",

    "SubmissionPeriods:1:SubmissionPeriod": "July to December 2023",
    "SubmissionPeriods:1:PeriodCodes:0": "2023-P3",
    "SubmissionPeriods:1:ErrorCode": "55",

    "SubmissionPeriods:2:SubmissionPeriod": "January to June 2024",
    "SubmissionPeriods:2:PeriodCodes:0": "2024-P1",
    "SubmissionPeriods:2:PeriodCodes:1": "2024-P2",
    "SubmissionPeriods:2:PeriodCodes:2": "2024-P3",
    "SubmissionPeriods:2:ErrorCode": "65",

    "SubmissionPeriods:3:SubmissionPeriod": "July to December 2024",
    "SubmissionPeriods:3:PeriodCodes:0": "2024-P4",
    "SubmissionPeriods:3:ErrorCode": "66"
  }
}
```


### Run
Go to `src/EPR.ProducerContentValidation.FunctionApp` directory and execute:

```
func start
```

### Docker

Run in terminal at the solution source root:

```
docker build -t producervalidation -f EPR.ProducerContentValidation.FunctionApp/Dockerfile . 
```

Fill out the environment variables and run the following command:
```
docker run -e AzureWebJobsStorage="X" -e FUNCTIONS_EXTENSION_VERSION="X" -e FUNCTIONS_WORKER_RUNTIME="X" -e Redis:ConnectionString="X" -e ServiceBus:ConnectionString="X" -e ServiceBus:SplitQueueName="X" -e StorageAccount:PomContainer="X" -e SubmissionApi:BaseUrl="X" -e Validation:Disabled="X" -e Validation:MaxIssuesToProces="X" producervalidation
```

### Redis

#### App settings
Add the following connection string under Values variables of local.settings.json/settings.json:
```
"Redis__ConnectionString": "localhost:6379"
```

#### To install Redis and Redis Stack
Recommended way of running Redis is to run it via Docker. In terminal run:
```
docker run -d --name epr-producers- -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

#### Inspect Redis keys in the session
To view the keys in Redis cache, open browser and point at: http://localhost:8001/redis-stack/browser

## How To Test

### Unit tests

On root directory `src`, execute:

```
dotnet test
```

### Pact tests

N/A

### Integration tests

N/A

## How To Debug

Use debugging tools in your chosen IDE.

## Environment Variables - deployed environments

The structure of the appsettings can be found in the repository. Example configurations for the different environments can be found in [epr-app-config-settings](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr-app-config-settings).

| Variable Name                  | Description                                                                                    |
| ------------------------------ |------------------------------------------------------------------------------------------------|
| AzureWebJobsStorage            | The connection string for the Azure Web Jobs Storage                                           |
| FUNCTIONS_EXTENSION_VERSION    | The extension version for Azure Function - i.e. ~4                                             |
| FUNCTIONS_WORKER_RUNTIME       | The runtime name for the Azure Function - i.e. `dotnet`                                        |
| Redis__ConnectionString        | THe connection string for Redis cache                                                          |
| ServiceBus__ConnectionString   | The connection string for the service bus                                                      |
| ServiceBus__SplitQueueName     | The name of the split queue                                                                    |
| StorageAccount__PomContainer   | The name of the blob container on the storage account, where uploaded POM files will be stored |
| SubmissionApi__BaseUrl         | The base URL for the Submission Status API WebApp                                              |
| Validation__Disabled           | Flag to disable validation                                                                     |
| Validation__MaxIssuesToProcess | Maximum number of validation Issues to be processed                                            |

## Additional Information

See [ADR-012.A: EPR Phase 1 - Compliance Scheme PoM Data Upload](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4251418625/ADR-012.A+EPR+Phase+1+-+Compliance+Scheme+PoM+Data+Upload)

### Monitoring and Health Check
Enable Health Check in the Azure portal and set the URL path to `ValidateProducerContent`

## Directory Structure

### Source files

- `EPR.ProducerContentValidation.Application`- Application .NET source files
- `EPR.ProducerContentValidation.Application.UnitTests` - Application .NET unit test files
- `EPR.ProducerContentValidation.FunctionApp`- Function .NET source files
- `Epr.ProducerContentValidation.FunctionApp.UnitTests` - Function .NET unit test files
- `EPR.ProducerContentValidation.TestSupport` - Application/Function .NET test support files

## Contributing to this project

Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).

test
