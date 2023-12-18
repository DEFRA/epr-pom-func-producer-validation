using EPR.ProducerContentValidation.Application.Models;
using EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators.Factories;

public class ProducerRowWarningValidatorFactory : IProducerRowWarningValidatorFactory
{
    private readonly IValidator<ProducerRow> _producerRowWarningValidator;

    public ProducerRowWarningValidatorFactory()
    {
        _producerRowWarningValidator = new ProducerRowWarningValidator();
    }

    public IValidator<ProducerRow> GetInstance() => _producerRowWarningValidator;
}