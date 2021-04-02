﻿using Confluent.Kafka;

namespace CecoChat.Kafka
{
    public interface IKafkaProducerOptions
    {
        string IDContext { get; }

        Acks Acks { get; }

        double LingerMs { get; }

        int MessageTimeoutMs { get; }

        int MessageSendMaxRetries { get; }
    }

    public sealed class KafkaProducerOptions : IKafkaProducerOptions
    {
        public string IDContext { get; set; }

        public Acks Acks { get; set; }

        public double LingerMs { get; set; }

        public int MessageTimeoutMs { get; set; }

        public int MessageSendMaxRetries { get; set; }
    }
}
