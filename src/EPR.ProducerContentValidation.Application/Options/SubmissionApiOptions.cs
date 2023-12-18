namespace EPR.ProducerContentValidation.Application.Options;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class SubmissionApiOptions
{
    public const string Section = "SubmissionApi";

    [Required]
    public string BaseUrl { get; set; }
}
