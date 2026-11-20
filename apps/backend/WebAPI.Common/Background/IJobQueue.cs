namespace WebAPI.Common.Background;

/// <summary>
/// Fila de trabalhos em processo (ex.: Channel&lt;T&gt;). Quem produz chama <see cref="EnqueueAsync"/>,
/// um BackgroundService consome com <see cref="DequeueAsync"/>.
/// </summary>
public interface IJobQueue<T>
{
    ValueTask EnqueueAsync(T item, CancellationToken cancellationToken = default);

    ValueTask<T> DequeueAsync(CancellationToken cancellationToken);
}
