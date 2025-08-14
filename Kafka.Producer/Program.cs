using Kafka.Producer;

Console.WriteLine("Hello, World!");


var kafkaService = new KafkaService();

// Multiple grouplara mesaj göndermek için use case 1.1 olarak isim güncelledik.
var topicName = "use-case-2-topic";

await kafkaService.CreateTopicAsync(topicName);
//await kafkaService.SendSimpleMessageWithNullKey(topicName);
await kafkaService.SendSimpleMessageWithIntKey(topicName);

Console.WriteLine("Messages sent.");