namespace EPR.ProducerContentValidation.Application.Options;

using System.ComponentModel.DataAnnotations;

public class RedisOptions
{
    public const string Section = "Redis";

    [Required]
    public string ConnectionString { get; set; }
}