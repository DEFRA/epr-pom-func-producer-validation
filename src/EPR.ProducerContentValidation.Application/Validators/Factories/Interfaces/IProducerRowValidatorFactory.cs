using EPR.ProducerContentValidation.Application.Models;
using FluentValidation;

namespace EPR.ProducerContentValidation.Application.Validators.Factories.Interfaces;

public interface IProducerRowValidatorFactory
{
    IValidator<ProducerRow> GetInstance();
}