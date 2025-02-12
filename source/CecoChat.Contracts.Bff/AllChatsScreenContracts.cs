using System.Text.Json.Serialization;
using Refit;

namespace CecoChat.Contracts.Bff;

public sealed class GetAllChatsScreenRequest
{
    [JsonPropertyName("chatsNewerThan")]
    [AliasAs("chatsNewerThan")]
    public DateTime ChatsNewerThan { get; init; }

    [JsonPropertyName("includeProfiles")]
    [AliasAs("includeProfiles")]
    public bool IncludeProfiles { get; init; }
}

public sealed class GetAllChatsScreenResponse
{
    [JsonPropertyName("chats")]
    [AliasAs("chats")]
    public ChatState[] Chats { get; init; } = Array.Empty<ChatState>();

    [JsonPropertyName("profiles")]
    [AliasAs("profiles")]
    public ProfilePublic[] Profiles { get; init; } = Array.Empty<ProfilePublic>();
}
