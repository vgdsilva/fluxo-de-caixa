using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAPI.Data.Entities;

namespace WebAPI.Data.Configuration;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => new { x.MessageId, x.Consumer });
        builder.Property(x => x.Consumer).HasMaxLength(200);
        builder.Property(x => x.Type).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
    }
}
