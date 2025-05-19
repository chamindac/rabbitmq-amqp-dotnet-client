using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using RabbitMQ.AMQP.Client;
using RabbitMQ.AMQP.Client.Impl;
using Trace = Amqp.Trace;
using TraceLevel = Amqp.TraceLevel;
using RMQ.Consumer;

Trace.TraceLevel = TraceLevel.Information;
ConsoleTraceListener consoleListener = new();
Trace.TraceListener = (l, f, a) =>
    consoleListener.WriteLine($"[{DateTime.Now}] [{l}] - {f}");

long messagesReceived = 0;
long messagesConfirmed = 0;
long notMessagesConfirmed = 0;
long messagesFailed = 0;

Task printStats = Task.Run(() =>
{
    while (true)
    {
        Trace.WriteLine(TraceLevel.Information, (
            $"[(Confirmed: {Interlocked.Read(ref messagesConfirmed)}, " +
            $"Failed: {Interlocked.Read(ref messagesFailed)}, UnConfirmed: {Interlocked.Read(ref notMessagesConfirmed)} )] " +
            $"[(Received: {Interlocked.Read(ref messagesReceived)})] " +
            $"(Un/Confirmed+Failed : {messagesConfirmed + messagesFailed + notMessagesConfirmed} ) "));
        Thread.Sleep(10000);
    }
});

Trace.WriteLine(TraceLevel.Information, "Starting");
const string containerId = "HA-Client-Connection";

ConnectionSettings rabbitmqConnectionSettings = ConnectionSettingsBuilder.Create()
                .User("rabbitmq_user")
                .Password("I1kMC-y2zsE2-rSAgEX3YiPT53Mr3dJR")
                .Host("localhost")
                .ContainerId(containerId)
                .Build();

IEnvironment environment = AmqpEnvironment.Create(rabbitmqConnectionSettings);

IConnection connection = await environment.CreateConnectionAsync();

connection.ChangeState += (sender, fromState, toState, e) =>
{
    Trace.WriteLine(TraceLevel.Information, $"Connection State Changed from {fromState} to {toState}");
};

Trace.WriteLine(TraceLevel.Information, "Connected");

const string queueName = "generatepreview-videogenerator";

ITranscoder transcoder = new Transcoder();

IConsumer consumer = await connection.ConsumerBuilder().Queue(queueName).InitialCredits(100).MessageHandler(async (context, message) =>
{
    Interlocked.Increment(ref messagesReceived);
    await transcoder.TranscodeAsync(message.BodyAsString()).ConfigureAwait(true); ;
    context.Accept();
}
).BuildAndStartAsync();

consumer.ChangeState += (sender, fromState, toState, e) =>
{
    Trace.WriteLine(TraceLevel.Information, $"Consumer State Changed, from {fromState} to {toState}");
};

Trace.WriteLine(TraceLevel.Information, "Queue Created");
Console.WriteLine("Press any key to delete the queue and close the connection.");
Console.ReadKey();

await consumer.CloseAsync();
consumer.Dispose();

await connection.CloseAsync();
connection.Dispose();

printStats.Dispose();
Trace.WriteLine(TraceLevel.Information, "Closed");