using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Kafka.Producer
{
    internal class KafkaService
    {
        private const string TopicName = "mytopic";
        internal async Task CreateTopicAsync()
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
    }
}
