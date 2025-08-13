using Kafka.Producer;

Console.WriteLine("Hello, World!");


var kafkaService = new KafkaService();

await kafkaService.CreateTopicAsync();