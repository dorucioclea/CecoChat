﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CecoChat.Client.Wpf.Infrastructure;
using CecoChat.Client.Wpf.Storage;
using CecoChat.Contracts.Client;
using Microsoft.Toolkit.Mvvm.Input;
using PropertyChanged;

namespace CecoChat.Client.Wpf.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public sealed class SingleChatViewModel : BaseViewModel
    {
        private readonly Dictionary<long, SingleChatMessageViewModel> _messageMap;

        public SingleChatViewModel(
            MessagingClient messagingClient,
            MessageStorage messageStorage,
            IDispatcher uiThreadDispatcher,
            IFeedbackService feedbackService)
            : base(messagingClient, messageStorage, uiThreadDispatcher, feedbackService)
        {
            _messageMap = new();

            MessagingClient.MessageReceived += MessagingClientOnMessageReceived;
            MessagingClient.MessageDelivered += MessagingClientOnMessageDelivered;

            Messages = new ObservableCollection<SingleChatMessageViewModel>();
            SendMessage = new AsyncRelayCommand(SendMessageExecuted);
        }

        public long OtherUserID { get; set; }

        public ObservableCollection<SingleChatMessageViewModel> Messages { get; }

        public bool CanSend { get; set; }

        public string MessageText { get; set; }

        public ICommand SendMessage { get; }

        public event EventHandler<ClientMessage> MessageSent;

        private void MessagingClientOnMessageReceived(object sender, ListenResponse response)
        {
            if (response.Message.SenderId != OtherUserID && response.Message.ReceiverId != OtherUserID)
            {
                return;
            }

            InsertMessage(response.Message, response.SequenceNumber);
        }

        private void MessagingClientOnMessageDelivered(object sender, ListenResponse response)
        {
            if (_messageMap.TryGetValue(response.Message.MessageId, out SingleChatMessageViewModel messageVM))
            {
                messageVM.SequenceNumber = response.SequenceNumber.ToString();
                messageVM.DeliveryStatus = response.Message.AckType.ToString();
            }
        }

        private async Task SendMessageExecuted()
        {
            try
            {
                ClientMessage message = await MessagingClient.SendPlainTextMessage(OtherUserID, MessageText);
                MessageStorage.AddMessage(OtherUserID, message);
                InsertMessage(message);
                MessageText = string.Empty;
                MessageSent?.Invoke(this, message);
            }
            catch (Exception exception)
            {
                FeedbackService.ShowError(exception);
            }
        }

        public async Task SetOtherUser(long otherUserID)
        {
            OtherUserID = otherUserID;

            _messageMap.Clear();
            Messages.Clear();

            IEnumerable<ClientMessage> storedMessages = MessageStorage.GetMessages(otherUserID);
            IList<ClientMessage> dialogHistory = await MessagingClient.GetHistory(otherUserID, DateTime.UtcNow);
            IEnumerable<ClientMessage> allMessages = storedMessages.Union(dialogHistory);

            foreach (ClientMessage message in allMessages)
            {
                InsertMessage(message);
            }

            CanSend = true;
        }

        private void InsertMessage(ClientMessage message, int? sequenceNumber = null)
        {
            long messageID = message.MessageId;
            DateTime messageTimestamp = message.MessageId.ToTimestamp();

            if (_messageMap.ContainsKey(messageID))
            {
                return;
            }

            int insertionIndex = 0;

            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                if (messageTimestamp > Messages[i].Timestamp)
                {
                    insertionIndex = i + 1;
                    break;
                }
            }

            SingleChatMessageViewModel messageVM = new()
            {
                IsSenderCurrentUser = message.SenderId == MessagingClient.UserID,
                Timestamp = messageTimestamp,
                FormattedMessage = $"[{messageTimestamp}] {message.SenderId}: {message.Text}",
                SequenceNumber = sequenceNumber?.ToString()
            };

            if (insertionIndex >= Messages.Count)
            {
                UIThreadDispatcher.Invoke(() => Messages.Add(messageVM));
            }
            else
            {
                UIThreadDispatcher.Invoke(() => Messages.Insert(insertionIndex, messageVM));
            }

            _messageMap.Add(messageID, messageVM);
        }
    }
}
