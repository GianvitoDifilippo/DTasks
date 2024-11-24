﻿using DTasks.Hosting;

namespace DTasks.Storage;

public interface IDAsyncStorage
{
    Task<ReadOnlyMemory<byte>> LoadAsync(DAsyncId id, CancellationToken cancellationToken = default);

    Task SaveAsync(DAsyncId id, ReadOnlyMemory<byte> bytes, CancellationToken cancellationToken = default);
}
