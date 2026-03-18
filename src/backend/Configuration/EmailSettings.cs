namespace PhotosMarket.API.Configuration;

public class EmailSettings
{
    public bool Enabled { get; set; } = false;
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
