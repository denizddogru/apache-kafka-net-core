using Kafka.Consumer;


Console.WriteLine("Kafka Consumer 1");
var topicName = "use-case-4-topic";

var kafkaService = new KafkaService();

//await kafkaService.ConsumeSimpleMessageWithNullKey(topicName);

//await kafkaService.ConsumeSimpleMessageWithIntKey(topicName);

//await kafkaService.ConsumeComplexMessageWithIntKey(topicName);

//await kafkaService.ConsumeComplexMessageWithIntKeyAndHeader(topicName);

await kafkaService.ConsumeMessageFromSpecificPartitionOffset(topicName);



Console.ReadLine();