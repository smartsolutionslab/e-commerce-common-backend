namespace E_Commerce.Common.Messaging.Configuration;

public class MessageBrokerConfig
{
    public const string SectionName = "MessageBroker";
    
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "admin123";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "ecommerce.events";
}
