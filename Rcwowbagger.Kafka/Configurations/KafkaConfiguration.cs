namespace Rcwowbagger.Kafka.Configurations;

internal class KafkaConfiguration
{
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string GroupId { get; set; }
    public string ClientId { get; set; }
}
