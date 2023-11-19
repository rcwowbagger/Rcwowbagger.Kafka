namespace Rcwowbagger.Kafka.Configurations;

public class KafkaConfiguration
{
    public string BootstrapServers { get; set; }
    public string TopicPrefix { get; set; }
    public string GroupId { get; set; }
    public string ClientId { get; set; }
    public bool? EnableAutoCommit { get; set; }
}
