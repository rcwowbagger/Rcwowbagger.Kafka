
using Microsoft.Extensions.Configuration;
using Rcwowbagger.Kafka;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var logger = Log.ForContext<Program>();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


var consumer = new KafkaConsumer(configuration);

CancellationTokenSource tokenSource = new CancellationTokenSource();
Task.Run(async () => consumer.SubscribeAsync(tokenSource.Token));

Console.ReadLine();
tokenSource.Cancel();