
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka;
using Rcwowbagger.Kafka.Configurations;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var logger = Log.ForContext<Program>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var config = configuration.GetSection("Kafka").Get<KafkaConfiguration>();


var consumer = new KafkaConsumer<string, string>(configuration);
consumer.OnMessage += (key, value) =>
{

    Console.WriteLine(value);
    //Console.WriteLine($"{obj} { (DateTime.Now - DateTime.Parse(obj)).TotalMilliseconds.ToString("n2") }");
};


var producer = new KafkaPublisher<string, string>(configuration);

CancellationTokenSource tokenSource = new CancellationTokenSource();
Task.Run(async () => consumer.SubscribeAsync(tokenSource.Token));
Task.Run(async () =>
{
    while (!tokenSource.IsCancellationRequested)
    {
        await producer.PublishAsync($"{DateTime.Now:o}", config.TopicPrefix + "1");
        await producer.PublishAsync("key", $"{DateTime.Now:o}", config.TopicPrefix + "2");
        await Task.Delay(1_000);
    }
});


Console.ReadLine();
tokenSource.Cancel();