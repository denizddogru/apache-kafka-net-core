# Apache Kafka Concepts - Visual Guide

Bu dokümanda Apache Kafka'nın temel kavramları görsel örneklerle açıklanmıştır.

## 📊 Kafka Hiyerarşisi

```
🏢 CLUSTER (Kafka Sistemi)
├── 🖥️ BROKER 1
├── 🖥️ BROKER 2  
└── 🖥️ BROKER 3
    ├── 📋 TOPIC: "customer-orders"
    │   ├── 📦 PARTITION 0
    │   ├── 📦 PARTITION 1
    │   ├── 📦 PARTITION 2
    │   └── 📦 PARTITION 3
    ├── 📋 TOPIC: "user-events"
    └── 📋 TOPIC: "payments"
```

### Temel Kavramlar

| Kavram | Açıklama | .NET Analojisi |
|--------|----------|----------------|
| **Cluster** | Tüm Kafka altyapısı | SQL Server Cluster |
| **Broker** | Cluster içindeki her sunucu | Server Instance |
| **Topic** | Mesaj kategorisi/kuyruğu | Database Table |
| **Partition** | Topic'in fiziksel parçaları | Table Partition |
| **Replication Factor** | Her partition'ın kaç kopyası | Database Replication |

## 🎯 Senaryo Karşılaştırması

### Replication Factor = 1 (RİSKLİ)

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   BROKER 1  │    │   BROKER 2  │    │   BROKER 3  │
├─────────────┤    ├─────────────┤    ├─────────────┤
│     P0      │    │     P1      │    │     P2      │
│     P3      │    │             │    │             │
└─────────────┘    └─────────────┘    └─────────────┘

⚠️  BROKER 1 çökerse → P0 ve P3 kaybolur!
```

### Replication Factor = 2 (GÜVENLİ)

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│    BROKER 1     │    │    BROKER 2     │    │    BROKER 3     │
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ P0 (Leader) 🟢  │    │ P1 (Leader) 🟢  │    │ P2 (Leader) 🟢  │
│ P1 (Follower)🔴 │    │ P2 (Follower)🔴 │    │ P0 (Follower)🔴 │
│ P3 (Leader) 🟢  │    │                 │    │ P3 (Follower)🔴 │
└─────────────────┘    └─────────────────┘    └─────────────────┘

✅ BROKER 1 çökse bile → P0 ve P3'ün kopyaları BROKER 3'te!
```

**Legend:**
- 🟢 **Leader**: Yazma/Okuma işlemleri
- 🔴 **Follower**: Backup kopyası

## 💻 .NET Confluent.Kafka Kullanımı

### Producer Konfigürasyonu

```csharp
using Confluent.Kafka;
using System.Text.Json;

// Cluster'a bağlantı
var config = new ProducerConfig 
{ 
    BootstrapServers = "broker1:9092,broker2:9092,broker3:9092",
    Acks = Acks.All,  // Tüm replica'lardan onay bekle
    Retries = 3
};

using var producer = new ProducerBuilder<string, string>(config).Build();
```

### Mesaj Gönderme

```csharp
public async Task SendOrderAsync(Order order)
{
    var message = new Message<string, string> 
    { 
        Key = order.CustomerId,  // Aynı customer → Aynı partition
        Value = JsonSerializer.Serialize(order),
        Headers = new Headers
        {
            { "eventType", Encoding.UTF8.GetBytes("OrderCreated") },
            { "version", Encoding.UTF8.GetBytes("1.0") }
        }
    };

    try 
    {
        var result = await producer.ProduceAsync("customer-orders", message);
        Console.WriteLine($"Mesaj gönderildi: Topic={result.Topic}, Partition={result.Partition}, Offset={result.Offset}");
    }
    catch (ProduceException<string, string> ex)
    {
        Console.WriteLine($"Hata: {ex.Error.Reason}");
    }
}
```

### Consumer Konfigürasyonu

```csharp
var consumerConfig = new ConsumerConfig
{
    BootstrapServers = "broker1:9092,broker2:9092,broker3:9092",
    GroupId = "order-processing-service",
    AutoOffsetReset = AutoOffsetReset.Earliest,
    EnableAutoCommit = false  // Manual commit
};

using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
consumer.Subscribe("customer-orders");

while (true)
{
    var consumeResult = consumer.Consume(TimeSpan.FromSeconds(1));
    
    if (consumeResult != null)
    {
        var order = JsonSerializer.Deserialize<Order>(consumeResult.Message.Value);
        
        // İş mantığı
        await ProcessOrderAsync(order);
        
        // Manual commit
        consumer.Commit(consumeResult);
        
        Console.WriteLine($"İşlenen mesaj: Partition={consumeResult.Partition}, Offset={consumeResult.Offset}");
    }
}
```

## 🔧 Partition Key Stratejisi

### Doğru Key Seçimi

```csharp
// ✅ DOĞRU: Aynı customer'ın mesajları sıralı
var message = new Message<string, string> 
{ 
    Key = order.CustomerId,  // "customer_123"
    Value = orderJson 
};

// ❌ YANLIŞ: Random key = Sıralama bozulur
var message = new Message<string, string> 
{ 
    Key = Guid.NewGuid().ToString(),
    Value = orderJson 
};
```

### Key'e Göre Partition Dağılımı

```
Key: "customer_123" → Hash → Partition 1
Key: "customer_456" → Hash → Partition 3  
Key: "customer_789" → Hash → Partition 0
Key: "customer_123" → Hash → Partition 1  // Aynı partition!
```

## 📈 Production Best Practices

### 1. Replication Factor
```yaml
# Minimum production ayarı
default.replication.factor=3
min.insync.replicas=2
```

### 2. Producer Ayarları
```csharp
var config = new ProducerConfig 
{ 
    BootstrapServers = connectionString,
    Acks = Acks.All,           // Durability
    Retries = int.MaxValue,    // Retry
    MaxInFlight = 5,           // Performance
    EnableIdempotence = true,  // Duplicate prevention
    CompressionType = CompressionType.Snappy
};
```

### 3. Consumer Group Pattern
```csharp
// Horizontal scaling için aynı group.id
var consumerConfig = new ConsumerConfig
{
    GroupId = "order-processing-service",  // Aynı service'in tüm instance'ları
    EnableAutoCommit = false,
    SessionTimeoutMs = 30000,
    HeartbeatIntervalMs = 10000
};
```

## 🚀 Gerçek Dünya Örneği: E-Commerce

### Topic Tasarımı
```
📋 customer-orders      (4 partitions, RF=3)
📋 inventory-updates    (2 partitions, RF=3)  
📋 payment-events       (6 partitions, RF=3)
📋 user-activities      (8 partitions, RF=2)
```

### Mikroservis Mimarisi
```csharp
// Order Service → Producer
await orderProducer.ProduceAsync("customer-orders", orderMessage);
await orderProducer.ProduceAsync("inventory-updates", inventoryMessage);

// Inventory Service → Consumer
consumer.Subscribe(new[] { "customer-orders", "inventory-updates" });

// Payment Service → Consumer + Producer  
paymentConsumer.Subscribe("customer-orders");
await paymentProducer.ProduceAsync("payment-events", paymentResult);
```

### Monitoring

```csharp
// Metrics collection
public class KafkaMetrics
{
    public void RecordProduceLatency(TimeSpan latency) { }
    public void RecordConsumeRate(int messagesPerSecond) { }
    public void RecordPartitionLag(int lag) { }
}
```

## 🛠️ Troubleshooting

### Yaygın Sorunlar

| Sorun | Sebep | Çözüm |
|-------|-------|--------|
| Yavaş consume | Consumer lag | Partition sayısını artır |
| Message loss | Acks=0 | Acks=All yap |
| Duplicate messages | Idempotence kapalı | EnableIdempotence=true |
| Out of order | Yanlış key | Key stratejisini gözden geçir |

### Performans Optimizasyonu

```csharp
// Batch processing
var config = new ProducerConfig 
{ 
    BatchSize = 16384,        // 16KB batch
    LingerMs = 5,             // 5ms bekle
    CompressionType = CompressionType.Snappy
};

// Async consumer
var tasks = new List<Task>();
while (running)
{
    var result = consumer.Consume(100);
    if (result != null)
    {
        tasks.Add(ProcessMessageAsync(result));
        
        if (tasks.Count >= 10)  // Batch commit
        {
            await Task.WhenAll(tasks);
            consumer.Commit();
            tasks.Clear();
        }
    }
}
```
