using Kafka.Producer;



var kafkaService = new KafkaService();

// Multiple grouplara mesaj göndermek için use case 1.1 olarak isim güncelledik.
var topicName = "use-case-3-topic";

await kafkaService.CreateTopicAsync(topicName);
//await kafkaService.SendSimpleMessageWithNullKey(topicName);
//await kafkaService.SendSimpleMessageWithIntKey(topicName);
//await kafkaService.SendComplexMessageWithIntKey(topicName);
await kafkaService.SendComplexMessageWithIntKeyAndHeader(topicName);


Console.WriteLine("Messages sent.");