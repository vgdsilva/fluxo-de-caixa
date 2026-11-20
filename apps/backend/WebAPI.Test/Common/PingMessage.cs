using WebAPI.Common.Messaging;

namespace WebAPI.Test.Messages;

[MessageType("test.ping")]
public sealed record PingMessage(string Text) : IMessage;
