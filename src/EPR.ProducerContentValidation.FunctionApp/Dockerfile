FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Build requires 3.1 SDK
#COPY --from=mcr.microsoft.com/dotnet/core/sdk:3.1 /usr/share/dotnet /usr/share/dotnet

# Copy everything
COPY stylecop.ruleset ./
COPY Directory.Build.props ./
COPY EPR.ProducerContentValidation.Application/. ./EPR.ProducerContentValidation.Application/.
COPY EPR.ProducerContentValidation.FunctionApp/. ./EPR.ProducerContentValidation.FunctionApp/.

RUN dotnet publish EPR.ProducerContentValidation.FunctionApp/*.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:4-appservice
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=installer-env --chown=dotnet ["/home/site/wwwroot", "/home/site/wwwroot"]