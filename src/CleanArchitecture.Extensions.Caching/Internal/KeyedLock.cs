using System.Collections.Concurrent;

namespace CleanArchitecture.Extensions.Caching.Internal;

/// <summary>
/// Provides keyed lock management with cleanup to avoid unbounded growth.
/// </summary>
internal sealed class KeyedLock
{
    internal sealed class LockEntry
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public readonly object SyncRoot = new();
        public int RefCount;
        public bool Removed;
    }

    private readonly ConcurrentDictionary<string, LockEntry> _locks = new(StringComparer.Ordinal);

    public Releaser? TryAcquire(string key, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(key);

        var entry = GetEntry(key);
        if (!entry.Semaphore.Wait(timeout))
        {
            ReleaseEntry(key, entry);
            return null;
        }

        return new Releaser(this, key, entry);
    }

    public async ValueTask<Releaser?> TryAcquireAsync(string key, TimeSpan timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(key);

        var entry = GetEntry(key);
        try
        {
            if (!await entry.Semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                ReleaseEntry(key, entry);
                return null;
            }
        }
        catch
        {
            ReleaseEntry(key, entry);
            throw;
        }

        return new Releaser(this, key, entry);
    }

    private LockEntry GetEntry(string key)
    {
        while (true)
        {
            var entry = _locks.GetOrAdd(key, _ => new LockEntry());

            lock (entry.SyncRoot)
            {
                if (entry.Removed)
                {
                    continue;
                }

                entry.RefCount++;
                return entry;
            }
        }
    }

    private void ReleaseEntry(string key, LockEntry entry)
    {
        lock (entry.SyncRoot)
        {
            entry.RefCount--;

            if (entry.RefCount == 0 && _locks.TryRemove(new KeyValuePair<string, LockEntry>(key, entry)))
            {
                entry.Removed = true;
                entry.Semaphore.Dispose();
            }
        }
    }

    internal readonly struct Releaser : IDisposable
    {
        private readonly KeyedLock? _owner;
        private readonly string? _key;
        private readonly LockEntry? _entry;

        internal Releaser(KeyedLock owner, string key, LockEntry entry)
        {
            _owner = owner;
            _key = key;
            _entry = entry;
        }

        public void Dispose()
        {
            if (_owner is null || _entry is null || _key is null)
            {
                return;
            }

            _entry.Semaphore.Release();
            _owner.ReleaseEntry(_key, _entry);
        }
    }
}
