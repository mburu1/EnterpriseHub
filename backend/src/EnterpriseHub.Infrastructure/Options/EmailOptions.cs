namespace EnterpriseHub.Infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = null!;
    public string SmtpPassword { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
}
