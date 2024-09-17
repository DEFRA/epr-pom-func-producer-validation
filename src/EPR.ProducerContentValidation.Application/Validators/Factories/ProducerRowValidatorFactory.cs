using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Options;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EPR.ProducerContentValidation.Application.Validators.Factories;

public class ProducerRowValidatorFactory : IProducerRowValidatorFactory
{
    private readonly IValidator<ProducerRow> _producerRowValidator;

    public ProducerRowValidatorFactory(IOptions<ValidationOptions> validationOptions)
    {
        if (validationOptions.Value.Disabled)
        {
            _producerRowValidator = new ProducerRowValidatorMinimal();
        }
        else
        {
            _producerRowValidator = validationOptions.Value.IsLatest ? new Producer13ColumnRowValidator() : new ProducerRowValidator();
        }
    }

    public IValidator<ProducerRow> GetInstance() => _producerRowValidator;
}