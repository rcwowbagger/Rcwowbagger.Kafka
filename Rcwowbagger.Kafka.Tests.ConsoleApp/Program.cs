﻿
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


var consumer = new KafkaConsumer<string>(configuration);
consumer.OnMessage += (obj) =>
{
    Console.WriteLine(obj.ToString());
};


var producer = new KafkaPublisher<string>(configuration);

CancellationTokenSource tokenSource = new CancellationTokenSource();
Task.Run(async () => consumer.SubscribeAsync(tokenSource.Token));
Task.Run(async () =>
{
    while (!tokenSource.IsCancellationRequested)
    {
        await producer.PublishAsync($"{DateTime.UtcNow:o}");
        
        await Task.Delay(1_000);
    }
});


Console.ReadLine();
tokenSource.Cancel();