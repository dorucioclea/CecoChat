﻿using ProtoBuf;

namespace CecoChat.Contracts.Backend
{
    [ProtoContract]
    public sealed class PlainTextMessage : Message
    {
        [ProtoMember(1)]
        public string Text { get; set; }
    }
}
