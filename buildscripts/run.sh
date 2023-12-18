#! /usr/bin/env bash

cd "$(dirname "$0")/../" || exit 1

cd src/EPR.ProducerContentValidation.FunctionApp || exit 1

func start --verbose