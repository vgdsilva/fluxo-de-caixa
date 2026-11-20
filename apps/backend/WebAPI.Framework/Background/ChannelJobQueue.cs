using System.Threading.Channels;
using WebAPI.Common.Background;

namespace WebAPI.Framework.Background;

public sealed class ChannelJobQueue<T> : IJobQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();

    public ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(item, cancellationToken);

    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAsync(cancellationToken);
}
