namespace EPR.ProducerContentValidation.Application.Options;

using System.ComponentModel.DataAnnotations;

public class ServiceBusOptions
{
    public const string Section = "ServiceBus";

    [Required]
    public string ConnectionString { get; set; }

    [Required]
    public string SplitQueueName { get; set; }
}