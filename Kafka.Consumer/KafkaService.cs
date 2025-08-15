using Confluent.Kafka;
using Kafka.Consumer.Events;

namespace Kafka.Consumer;

internal class KafkaService
{
    internal async Task ConsumeSimpleMessageWithNullKey(string topicName)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9094",
            GroupId = "use-case-1-group-1",

            // AutoOffsetReset determines what to do when there is no initial offset in Kafka or if the current offset does not exist.
            // AutoOffsetReset.Earliest means the consumer will start reading from the earliest available message in the topic.
            // This is useful when you want to process all existing messages from the beginning.
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var consumer = new ConsumerBuilder<Null, string>(config).Build();
        consumer.Subscribe(topicName);

        while (true)
        {
            var consumeResult = consumer.Consume(5000); // Topicde kaç tane msj varsa, memory'ye dolduruyor. Mesajlar bitene kadar kafkaya uğramıyor tekrardan.

            if (consumeResult != null)
            {
                Console.WriteLine($"Incoming message: {consumeResult.Message.Value} ");
            }

            await Task.Delay(500);

        }


    }

    internal async Task ConsumeSimpleMessageWithIntKey(string topicName)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9094",
            GroupId = "use-case-2-group-1",

            // AutoOffsetReset determines what to do when there is no initial offset in Kafka or if the current offset does not exist.
            // AutoOffsetReset.Earliest means the consumer will start reading from the earliest available message in the topic.
            // This is useful when you want to process all existing messages from the beginning.
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var consumer = new ConsumerBuilder<int, string>(config).Build();
        consumer.Subscribe(topicName);

        while (true)
        {
            var consumeResult = consumer.Consume(5000); // Topicde kaç tane msj varsa, memory'ye dolduruyor. Mesajlar bitene kadar kafkaya uğramıyor tekrardan.

            if (consumeResult != null)
            {
                Console.WriteLine($"Incoming message: Key: {consumeResult.Message.Key} {consumeResult.Message.Value} Value: {consumeResult.Message.Value} ");
            }

            await Task.Delay(10);

        }


    }

    internal async Task ConsumeComplexMessageWithIntKey(string topicName)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9094",
            GroupId = "use-case-3-group-1",

            // AutoOffsetReset determines what to do when there is no initial offset in Kafka or if the current offset does not exist.
            // AutoOffsetReset.Earliest means the consumer will start reading from the earliest available message in the topic.
            // This is useful when you want to process all existing messages from the beginning.
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var consumer = new ConsumerBuilder<int, OrderCreatedEvent>(config)
            .SetValueDeserializer(new CustomValueDeserializer<OrderCreatedEvent>())
            .Build();
        consumer.Subscribe(topicName);

        while (true)
        {
            var consumeResult = consumer.Consume(5000); // Topicde kaç tane msj varsa, memory'ye dolduruyor. Mesajlar bitene kadar kafkaya uğramıyor tekrardan.

            var orderCreatedEvent = consumeResult.Message.Value;
            if (consumeResult != null)
            {
                Console.WriteLine($"Incoming message: {orderCreatedEvent.UserId} -- {orderCreatedEvent.OrderCode} -- {orderCreatedEvent.TotalPrice}");


                await Task.Delay(10);

            }


        }


    }

    internal async Task ConsumeComplexMessageWithIntKeyAndHeader(string topicName)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9094",
            GroupId = "use-case-3-group-1",

            // AutoOffsetReset determines what to do when there is no initial offset in Kafka or if the current offset does not exist.
            // AutoOffsetReset.Earliest means the consumer will start reading from the earliest available message in the topic.
            // This is useful when you want to process all existing messages from the beginning.
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var consumer = new ConsumerBuilder<int, OrderCreatedEvent>(config)
            .SetValueDeserializer(new CustomValueDeserializer<OrderCreatedEvent>())
            .Build();
        consumer.Subscribe(topicName);

        while (true)
        {
            var consumeResult = consumer.Consume(5000); // Topicde kaç tane msj varsa, memory'ye dolduruyor. Mesajlar bitene kadar kafkaya uğramıyor tekrardan.


            if (consumeResult != null)
            {

                var correlationId2  = consumeResult.Message.Headers[0].GetValueBytes();
                var version2 = consumeResult.Message.Headers[1].GetValueBytes();
                
                Console.WriteLine($"headers: correlationId: {correlationId2}, version: {version2}");


                var orderCreatedEvent = consumeResult.Message.Value;
                Console.WriteLine($"Incoming message: {orderCreatedEvent.UserId} -- {orderCreatedEvent.OrderCode} -- {orderCreatedEvent.TotalPrice}");


                await Task.Delay(10);

            }


        }


    }

    internal async Task ConsumeMessageFromSpecificPartitionOffset(string topicName)
    {
        var config = new ConsumerConfig()
        {
            BootstrapServers = "localhost:9094",
            GroupId = "group-4",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        var consumer = new ConsumerBuilder<Null, string>(config).Build();
        //consumer.Subscribe(topicName);
        consumer.Assign(new TopicPartitionOffset(topicName, 2, 0));
        while (true)
        {
            var consumeResult = consumer.Consume(5000);

            if (consumeResult != null)
            {
                Console.WriteLine($"Message Timestamp : {consumeResult.Message.Value}");
            }

            await Task.Delay(10);
        }
    }


}
