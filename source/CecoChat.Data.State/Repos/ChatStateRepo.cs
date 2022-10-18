using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using CecoChat.Contracts.State;
using CecoChat.Data.State.Instrumentation;
using Microsoft.Extensions.Logging;

namespace CecoChat.Data.State.Repos;

public interface IChatStateRepo
{
    void Prepare();

    Task<IReadOnlyCollection<ChatState>> GetChats(long userID, DateTime newerThan);

    ChatState GetChat(long userID, string chatID);

    void UpdateChat(long userID, ChatState chat);
}

internal class ChatStateRepo : IChatStateRepo
{
    private readonly ILogger _logger;
    private readonly IStateActivityUtility _stateActivityUtility;
    private readonly IStateDbContext _dbContext;
    private readonly Lazy<PreparedStatement> _chatsQuery;
    private readonly Lazy<PreparedStatement> _chatQuery;
    private readonly Lazy<PreparedStatement> _updateQuery;

    public ChatStateRepo(
        ILogger<ChatStateRepo> logger,
        IStateActivityUtility stateActivityUtility,
        IStateDbContext dbContext)
    {
        _logger = logger;
        _stateActivityUtility = stateActivityUtility;
        _dbContext = dbContext;

        _chatsQuery = new Lazy<PreparedStatement>(() => _dbContext.PrepareQuery(SelectNewerChatsForUser));
        _chatQuery = new Lazy<PreparedStatement>(() => _dbContext.PrepareQuery(SelectChatForUser));
        _updateQuery = new Lazy<PreparedStatement>(() => _dbContext.PrepareQuery(UpdateChatForUser));
    }

    private const string SelectNewerChatsForUser =
        "SELECT chat_id, newest_message, other_user_delivered, other_user_seen " +
        "FROM user_chats " +
        "WHERE user_id = ? AND newest_message > ? ALLOW FILTERING";
    private const string SelectChatForUser =
        "SELECT newest_message, other_user_delivered, other_user_seen " +
        "FROM user_chats " +
        "WHERE user_id = ? AND chat_id = ?;";
    private const string UpdateChatForUser =
        "INSERT into user_chats " +
        "(user_id, chat_id, newest_message, other_user_delivered, other_user_seen) " +
        "VALUES (?, ?, ?, ?, ?);";

    public void Prepare()
    {
        PreparedStatement _ = _chatsQuery.Value;
#pragma warning disable IDE0059
        PreparedStatement __ = _chatQuery.Value;
        PreparedStatement ___ = _updateQuery.Value;
#pragma warning restore IDE0059
    }

    public async Task<IReadOnlyCollection<ChatState>> GetChats(long userID, DateTime newerThan)
    {
        Activity activity = _stateActivityUtility.StartGetChats(_dbContext.Session, userID);
        bool success = false;

        try
        {
            long newerThanSnowflake = newerThan.ToSnowflakeFloor();
            BoundStatement query = _chatsQuery.Value.Bind(userID, newerThanSnowflake);
            query.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);
            query.SetIdempotence(true);

            RowSet rows = await _dbContext.Session.ExecuteAsync(query);
            List<ChatState> chats = new();

            foreach (Row row in rows)
            {
                ChatState chat = new();

                chat.ChatId = row.GetValue<string>("chat_id");
                chat.NewestMessage = row.GetValue<long>("newest_message");
                chat.OtherUserDelivered = row.GetValue<long>("other_user_delivered");
                chat.OtherUserSeen = row.GetValue<long>("other_user_seen");

                chats.Add(chat);
            }
            success = true;

            _logger.LogTrace("Returned {0} chats for user {1} which are newer than {2}.", chats.Count, userID, newerThan);
            return chats;
        }
        finally
        {
            _stateActivityUtility.Stop(activity, success);
        }
    }

    public ChatState GetChat(long userID, string chatID)
    {
        Activity activity = _stateActivityUtility.StartGetChat(_dbContext.Session, userID, chatID);
        bool success = false;

        try
        {
            BoundStatement query = _chatQuery.Value.Bind(userID, chatID);
            query.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);
            query.SetIdempotence(true);

            RowSet rows = _dbContext.Session.Execute(query);
            Row row = rows.FirstOrDefault();
            ChatState chat = null;
            if (row != null)
            {
                chat = new();

                chat.ChatId = chatID;
                chat.NewestMessage = row.GetValue<long>("newest_message");
                chat.OtherUserDelivered = row.GetValue<long>("other_user_delivered");
                chat.OtherUserSeen = row.GetValue<long>("other_user_seen");

                _logger.LogTrace("Returned chat {0} for user {1}.", chatID, userID);
            }
            else
            {
                _logger.LogTrace("Failed to find chat {0} for user {1}.", chatID, userID);
            }

            success = true;
            return chat;
        }
        finally
        {
            _stateActivityUtility.Stop(activity, success);
        }
    }

    public void UpdateChat(long userID, ChatState chat)
    {
        Activity activity = _stateActivityUtility.StartUpdateChat(_dbContext.Session, userID, chat.ChatId);
        bool success = false;

        try
        {
            BoundStatement query = _updateQuery.Value.Bind(userID, chat.ChatId, chat.NewestMessage, chat.OtherUserDelivered, chat.OtherUserSeen);
            query.SetConsistencyLevel(ConsistencyLevel.LocalQuorum);
            query.SetIdempotence(false);

            _dbContext.Session.Execute(query);
            success = true;
            _logger.LogTrace("Updated chat {0} for user {1}.", chat.ChatId, userID);
        }
        finally
        {
            _stateActivityUtility.Stop(activity, success);
        }
    }
}