using System.Collections.Concurrent;
using OpenTK.Mathematics;

namespace Minecraft.World;

public class ChunkGenerator : IDisposable
{
    private readonly ConcurrentQueue<ChunkGenerationTask> _queue = new();
    private readonly ConcurrentQueue<ChunkGenerationResult> _results = new();
    private readonly Thread _workerThread;
    private readonly CancellationTokenSource _cts = new();
    private readonly AutoResetEvent _signal = new(false);

    public ChunkGenerator()
    {
        _workerThread = new Thread(WorkerLoop) { IsBackground = true, Name = "ChunkGenerator" };
        _workerThread.Start();
    }

    public void EnqueueTask(ChunkGenerationTask task)
    {
        _queue.Enqueue(task);
        _signal.Set();
    }

    public bool TryDequeueResult(out ChunkGenerationResult result)
    {
        return _results.TryDequeue(out result);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _signal.Set();
        if (_workerThread.IsAlive && !_workerThread.Join(2000))
            _workerThread.Interrupt();
        _cts.Dispose();
        _signal.Dispose();
    }

    private void WorkerLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            while (_queue.TryDequeue(out ChunkGenerationTask task))
            {
                try
                {
                    Chunk chunk = new(task.ChunkPosition);
                    chunk.Generate();
                    _results.Enqueue(new ChunkGenerationResult
                    {
                        ChunkPosition = task.ChunkPosition,
                        Chunk = chunk,
                        Success = true
                    });
                }
                catch (Exception)
                {
                    _results.Enqueue(new ChunkGenerationResult
                    {
                        ChunkPosition = task.ChunkPosition,
                        Chunk = null,
                        Success = false
                    });
                }
            }
            _signal.WaitOne(100);
        }
    }
}

public struct ChunkGenerationTask
{
    public Vector3i ChunkPosition;
}

public struct ChunkGenerationResult
{
    public Vector3i ChunkPosition;
    public Chunk? Chunk;
    public bool Success;
}
