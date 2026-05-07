using System.Collections.Concurrent;
using OpenTK.Mathematics;

namespace Minecraft.Graphics;

public class MeshWorker : IDisposable
{
    private readonly ConcurrentQueue<MeshTask> _queue = new();
    private readonly ConcurrentQueue<MeshResult> _results = new();
    private readonly Thread _workerThread;
    private readonly CancellationTokenSource _cts = new();
    private readonly AutoResetEvent _signal = new(false);

    public MeshWorker()
    {
        _workerThread = new Thread(WorkerLoop) { IsBackground = true, Name = "MeshingWorker" };
        _workerThread.Start();
    }

    public void EnqueueTask(MeshTask task)
    {
        _queue.Enqueue(task);
        _signal.Set();
    }

    public bool TryDequeueResult(out MeshResult result)
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
            while (_queue.TryDequeue(out MeshTask task))
            {
                try
                {
                    float[] vertices = NativeModule.MeshChunk(task.ChunkPosition, task.Blocks);
                    _results.Enqueue(new MeshResult
                    {
                        ChunkPosition = task.ChunkPosition,
                        Vertices = vertices,
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    _results.Enqueue(new MeshResult
                    {
                        ChunkPosition = task.ChunkPosition,
                        Vertices = [],
                        Success = false
                    });
                }
            }
            _signal.WaitOne(100);
        }
    }
}
