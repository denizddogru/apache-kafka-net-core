using Kafka.Consumer;


Console.WriteLine("Kafka Consumer 1");
var topicName = "use-case-3-topic";

var kafkaService = new KafkaService();

//await kafkaService.ConsumeSimpleMessageWithNullKey(topicName);

//await kafkaService.ConsumeSimpleMessageWithIntKey(topicName);

await kafkaService.ConsumeComplexMessageWithIntKey(topicName);

Console.ReadLine();