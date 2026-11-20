using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Data.Entities;

namespace WebAPI.Data.Configuration;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
    }
}
