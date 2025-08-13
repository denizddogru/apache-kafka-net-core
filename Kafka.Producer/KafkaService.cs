using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Kafka.Producer
{
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
            new TopicSpecification() {  Name = TopicName, NumPartitions = 3, ReplicationFactor = 1}
                });

                Console.WriteLine($"Topic({TopicName}) has been created.");
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

            foreach(var item in Enumerable.Range(1,10))
            {

                var message = new Message<Null, string>()
                {
                    Value = $"Message(use case -1) - {item}"
                };

                var result = await producer.ProduceAsync(TopicName, message);


                foreach(var propertyInfo in result.GetType().GetProperties())
                {
                    Console.WriteLine($"{propertyInfo.Name} : {propertyInfo.GetValue(result)}");
                    await Task.Delay(200);
                }

                Console.WriteLine("-----------------------------------");
            }


        }
    }
}
