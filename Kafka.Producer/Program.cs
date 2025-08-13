using Kafka.Producer;

Console.WriteLine("Hello, World!");


var kafkaService = new KafkaService();

var topicName = "use-case-1-topic";

await kafkaService.CreateTopicAsync(topicName);
await kafkaService.SendSimpleMessageWithNullKey(topicName);

Console.WriteLine("Messages sent.");