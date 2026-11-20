namespace WebAPI.Common.Messaging;

/// <summary>
/// Grava a mensagem na tabela de outbox, na mesma transação do DbContext.
/// Ela só é publicada depois do SaveChanges (padrão Transactional Outbox).
/// </summary>
public interface IOutbox
{
    void Add<TMessage>(TMessage message, string? correlationId = null) where TMessage : IMessage;
}
