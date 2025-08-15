using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Kafka.Producer.Events;
using System.Text;

namespace Kafka.Producer;

internal class KafkaService
{
    private const string TopicName = "mytopic";
    internal async Task CreateTopicAsync(string topicName)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig()
        {
            BootstrapServers = "localhost:9094" // ilerde appsettingsden okunacak, 3 brokerlı bir kafka olsaydı 3'ünün de adresini yazabilirdik

        }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
        new TopicSpecification() {  Name = topicName, NumPartitions = 6, ReplicationFactor = 1}
            });

            Console.WriteLine($"Topic({topicName}) has been created.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    /// <summary>
    /// Sends a series of simple messages with a null key to a Kafka topic. Messages are built as key,value pairs in kafka. Key values are nullable.
    /// </summary>
    /// <remarks>This method demonstrates the use of a Kafka producer to send messages with a null
    /// key. The messages are sent to the Kafka cluster specified in the producer configuration.</remarks>
    /// <returns></returns>
    /// 

    internal async Task SendSimpleMessageWithNullKey(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094" };

        using var producer = new ProducerBuilder<Null, string>(config).Build();

        foreach (var item in Enumerable.Range(1, 10))
        {

            var message = new Message<Null, string>()
            {
                Value = $"Message(use case -1) - {item}"
            };

            var result = await producer.ProduceAsync(topicName, message);


            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");
                await Task.Delay(200);
            }

            Console.WriteLine("-----------------------------------");
        }


    }
    internal async Task SendSimpleMessageWithIntKey(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094" };

        using var producer = new ProducerBuilder<int, string>(config).Build();

        foreach (var item in Enumerable.Range(1, 100))
        {

            var message = new Message<int, string>()
            {
                Value = $"Message(use case -1) - {item}",
                Key = item
            };

            var result = await producer.ProduceAsync(topicName, message);


            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");

            }

            Console.WriteLine("-----------------------------------");
            await Task.Delay(10);
        }


    }
    internal async Task SendComplexMessageWithIntKey(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094" };

        using var producer = new ProducerBuilder<int, OrderCreatedEvent>(config)
            .SetValueSerializer(new CustomValueSerializer<OrderCreatedEvent>())
            .Build();

        foreach (var item in Enumerable.Range(1, 100))
        {
            var orderCreatedEvent = new OrderCreatedEvent() { OrderCode = Guid.NewGuid().ToString(), TotalPrice = item * 100, UserId = item };

            var message = new Message<int, OrderCreatedEvent>()
            {
                Value = orderCreatedEvent,
                Key = item
            };

            var result = await producer.ProduceAsync(topicName, message);


            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");

            }

            Console.WriteLine("-----------------------------------");
            await Task.Delay(10);
        }


    }
    internal async Task SendComplexMessageWithIntKeyAndHeader(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094" };

        using var producer = new ProducerBuilder<int, OrderCreatedEvent>(config)
            .SetValueSerializer(new CustomValueSerializer<OrderCreatedEvent>())
            .Build();

        foreach (var item in Enumerable.Range(1, 3))
        {
            var orderCreatedEvent = new OrderCreatedEvent() { OrderCode = Guid.NewGuid().ToString(), TotalPrice = item * 100, UserId = item };

            var header = new Headers
            {
                { "correlation_id", Encoding.UTF8.GetBytes("123") },
                { "version", Encoding.UTF8.GetBytes("v1") }
            };


            var message = new Message<int, OrderCreatedEvent>()
            {
                Value = orderCreatedEvent,
                Key = item,
                Headers = header
            };

            var result = await producer.ProduceAsync(topicName, message);


            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");

            }

            Console.WriteLine("-----------------------------------");
            await Task.Delay(10);
        }


    }
    
    /// <summary>
    /// This method selects the partition that will be read . ( In default Kafka distributes the messages to the partitions randomly.)
    /// (Number of consumers cannot exceed the number of partitions, in that case they the overexceeding consumer will not consume any messages.)
    /// </summary>
    /// <param name="topicName"></param>
    /// <returns></returns>
    internal async Task SendMessageToSpecificPartition(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094" };

        using var producer = new ProducerBuilder<Null, string>(config).Build();


        foreach (var item in Enumerable.Range(1, 10))
        {


            var message = new Message<Null, string>()
            {
                Value = $"Message: {item}"
            };

            var topicPartition = new TopicPartition(topicName, new Partition(2));

            var result = await producer.ProduceAsync(topicPartition, message);

            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");
            }

            Console.WriteLine("**********************");
            await Task.Delay(10);
        }

    }

    /// <summary>
    /// Sends message with Acknowledgement: 0->fire and forget, performance focus, low latency no retry
    /// 1->send to leader, waits for ack , 
    /// 2-> send to all, if there are multiple replicas messages are also saved to replicas, persists and prevents data loss
    /// </summary>
    /// <param name="topicName"></param>
    /// <returns></returns>
    internal async Task SendMessageWithAcknowledgement(string topicName)
    {
        var config = new ProducerConfig() { BootstrapServers = "localhost:9094", Acks = Acks.Leader };

        using var producer = new ProducerBuilder<Null, string>(config).Build();


        foreach (var item in Enumerable.Range(1, 10))
        {


            var message = new Message<Null, string>()
            {
                Value = $"Message: {item}"
            };

            var topicPartition = new TopicPartition(topicName, new Partition(2));

            var result = await producer.ProduceAsync(topicPartition, message);

            foreach (var propertyInfo in result.GetType().GetProperties())
            {
                Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");
            }

            Console.WriteLine("**********************");
            await Task.Delay(10);
        }

    }
}
