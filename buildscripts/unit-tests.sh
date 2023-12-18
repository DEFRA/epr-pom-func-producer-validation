#!/usr/bin/env bash

dotnet test src/EPR.ProducerContentValidation.FunctionApp.Tests/EPR.ProducerContentValidation.FunctionApp.Tests.csproj --logger "trx;logfilename=testResults.Function.trx"
dotnet test src/EPR.ProducerContentValidation.Application.Tests/EPR.ProducerContentValidation.Application.Tests.csproj --logger "trx;logfilename=testResults.Application.trx"