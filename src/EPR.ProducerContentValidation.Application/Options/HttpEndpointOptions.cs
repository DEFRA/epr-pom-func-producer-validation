namespace EPR.ProducerContentValidation.Application.Options;

public class HttpEndpointOptions
{
    public const string Section = "HttpEndpoint";

    /// <summary>
    /// Gets or sets whether the HTTP endpoint for local testing is enabled.
    /// Should be set to false in production and higher environments.
    /// </summary>
    public bool Enabled { get; set; } = false;
}