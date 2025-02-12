using CecoChat.Contracts.Messaging;

namespace CecoChat.ConsoleClient.LocalStorage;

public class ChangeHandler
{
    private readonly MessageStorage _storage;

    public ChangeHandler(MessageStorage storage)
    {
        _storage = storage;
    }

    public void AddReceivedMessage(ListenNotification notification)
    {
        if (notification.Type != MessageType.Data)
        {
            throw new InvalidOperationException($"Notification {notification} should have type {MessageType.Data}.");
        }
        if (notification.Data == null)
        {
            throw new InvalidOperationException($"Notification {notification} should have its {nameof(ListenNotification.Data)} property not null.");
        }

        Message message = new()
        {
            MessageId = notification.MessageId,
            SenderId = notification.SenderId,
            ReceiverId = notification.ReceiverId
        };

        switch (notification.Data.Type)
        {
            case Contracts.Messaging.DataType.PlainText:
                message.DataType = DataType.PlainText;
                message.Data = notification.Data.Data;
                break;
            default:
                throw new EnumValueNotSupportedException(notification.Data.Type);
        }

        _storage.AddMessage(message);
    }

    public void UpdateDeliveryStatus(ListenNotification notification)
    {
        if (notification.Type != MessageType.DeliveryStatus)
        {
            throw new InvalidOperationException($"Notification {notification} should have type {MessageType.DeliveryStatus}.");
        }

        if (!_storage.TryGetChat(notification.SenderId, notification.ReceiverId, out Chat? chat))
        {
            long otherUserId = _storage.GetOtherUserId(notification.SenderId, notification.ReceiverId);
            chat = new Chat(otherUserId)
            {
                NewestMessage = notification.MessageId
            };
            _storage.AddOrUpdateChat(chat);
        }

        switch (notification.DeliveryStatus)
        {
            case DeliveryStatus.Processed:
                chat.Processed = notification.MessageId;
                break;
            case DeliveryStatus.Delivered:
                chat.OtherUserDelivered = notification.MessageId;
                break;
            case DeliveryStatus.Seen:
                chat.OtherUserSeen = notification.MessageId;
                break;
            default:
                throw new EnumValueNotSupportedException(notification.DeliveryStatus);
        }
    }

    public void UpdateReaction(ListenNotification notification)
    {
        if (notification.Type != MessageType.Reaction)
        {
            throw new InvalidOperationException($"Notification {notification} should have type {MessageType.Reaction}.");
        }
        if (notification.Reaction == null)
        {
            throw new InvalidOperationException($"Notification {notification} should have its {nameof(ListenNotification.Reaction)} property not null.");
        }

        if (!_storage.TryGetMessage(notification.SenderId, notification.ReceiverId, notification.MessageId, out Message? message))
        {
            // the message is not in the local history
            return;
        }

        if (string.IsNullOrWhiteSpace(notification.Reaction.Reaction))
        {
            if (message.Reactions.ContainsKey(notification.Reaction.ReactorId))
            {
                message.Reactions.Remove(notification.Reaction.ReactorId);
            }
        }
        else
        {
            message.Reactions.Add(notification.Reaction.ReactorId, notification.Reaction.Reaction);
        }
    }
}
