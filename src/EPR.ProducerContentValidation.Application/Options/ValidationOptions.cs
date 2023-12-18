namespace EPR.ProducerContentValidation.Application.Options;

public class ValidationOptions
{
    public const string Section = "Validation";

    public bool Disabled { get; set; }

    public int MaxIssuesToProcess { get; set; }
}
