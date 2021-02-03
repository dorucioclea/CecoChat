﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CecoChat.Client.Shared;
using CecoChat.Contracts.Client;

namespace CecoChat.Client.Console
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            System.Console.Write("Your ID: ");
            long userID = long.Parse(System.Console.ReadLine() ?? string.Empty);

            MessagingClient client = new(new MessageIDGenerator());
            client.Initialize(userID, messagingServer: "https://localhost:31001", historyServer: "https://localhost:31003");
            client.MessageReceived += (_, message) => DisplayMessage(message);
            client.ExceptionOccurred += (_, exception) => System.Console.WriteLine(exception);

            client.ListenForMessages(CancellationToken.None);
            await ShowHistory(client);
            await Interact(client);

            client.Dispose();
            System.Console.WriteLine("Bye!");
        }

        private static async Task ShowHistory(MessagingClient client)
        {
            IList<ClientMessage> messageHistory = await client.GetUserHistory(DateTime.UtcNow);

            System.Console.WriteLine("{0} messages from history:", messageHistory.Count);
            foreach (ClientMessage message in messageHistory)
            {
                DisplayMessage(message);
            }
        }

        private static async Task Interact(MessagingClient client)
        {
            while (true)
            {
                System.Console.WriteLine("Receiver ID:");
                int receiverID = int.Parse(System.Console.ReadLine() ?? "0");
                if (receiverID <= 0)
                {
                    break;
                }

                System.Console.WriteLine("Message to {0}:", receiverID);
                string text = System.Console.ReadLine();

                try
                {
                    ClientMessage message = await client.SendPlainTextMessage(receiverID, text);
                    DisplayMessage(message);
                }
                catch (Exception exception)
                {
                    System.Console.WriteLine(exception);
                }
            }
        }

        private static void DisplayMessage(ClientMessage message)
        {
            System.Console.WriteLine($"[{message.Timestamp.ToDateTime():F}] {message.SenderId}->{message.ReceiverId}: {message.PlainTextData.Text}");
        }
    }
}