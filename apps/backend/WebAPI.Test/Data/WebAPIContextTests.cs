using Microsoft.EntityFrameworkCore;
using WebAPI.Data.Entities;
using WebAPI.Data.Repository;
using WebAPI.Test.Messages;

namespace WebAPI.Test.Data;

public sealed class WebAPIContextTests(PostgresFixture postgres) : IClassFixture<PostgresFixture>
{
    private void RequirePostgres() =>
        Assert.SkipUnless(postgres.IsAvailable, $"Defina {PostgresFixture.EnvironmentVariable} para rodar testes de banco.");

    [Fact]
    public async Task Outbox_grava_mensagem_junto_com_SaveChanges()
    {
        RequirePostgres();
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();

        var text = Guid.NewGuid().ToString();
        new Outbox(db).Add(new PingMessage(text));
        await db.SaveChangesAsync(ct);

        var saved = await db.OutboxMessages.SingleAsync(x => x.Payload.Contains(text), ct);
        Assert.Equal("test.ping", saved.Type);
        Assert.Null(saved.ProcessedAt);
    }

    [Fact]
    public async Task Assinatura_grava_lista_de_eventos_e_filtra_por_data()
    {
        RequirePostgres();
        var ct = TestContext.Current.CancellationToken;
        await using var db = postgres.CreateContext();

        var name = Guid.NewGuid().ToString("N");
        db.WebhookSubscriptions.Add(new WebhookSubscription
        {
            Name = name, Url = "https://exemplo.com", Secret = "s", EventTypes = ["lancamento.criado", "*"],
        });
        await db.SaveChangesAsync(ct);

        var limite = DateTimeOffset.UtcNow.AddMinutes(1);
        var saved = await db.WebhookSubscriptions.AsNoTracking()
            .SingleAsync(x => x.Name == name && x.CreatedAt <= limite, ct);

        Assert.Equal(["lancamento.criado", "*"], saved.EventTypes);
    }
}
