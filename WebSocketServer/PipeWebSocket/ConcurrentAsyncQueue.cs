using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    public class ConcurrentAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = null;
        public Action<T> OnProcess = null;
        public Action<T, Exception> OnException = null;
        private int isProcessing;   //队列是否正在处理数据
        private const int Processing = 1;   //有线程正在处理数据
        private const int UnProcessing = 0;   //没有线程处理数据
        private CancellationTokenSource cts = null;  //线程退出令牌
        public int ThreadCount { get; }  //消费线程数量

        public ConcurrentAsyncQueue(int threadCount = 1)
        {
            ThreadCount = threadCount;
            queue = new ConcurrentQueue<T>();
        }

        public bool IsRunning
        {
            get => Interlocked.CompareExchange(ref isProcessing, Processing, UnProcessing) == Processing;
        }

        public int Count { get => queue.Count; }

        public void Push(T obj)
        {
            queue.Enqueue(obj);

            if (!IsRunning) CreateTasks();
        }

        private void Listening()
        {
            while (queue.Count > 0)
            {
                if (queue.TryDequeue(out T item))
                {
                    try
                    {
                        OnProcess?.Invoke(item);
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(item, ex);
                    }
                }
            }
        }

        private async void CreateTasks()
        {
            cts = new CancellationTokenSource();
            Task[] tasks = new Task[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                var task = Task.Factory.StartNew(() => Listening(), cts.Token);
                tasks[i] = task;
            }

            await Task.WhenAll(tasks);  //等待消费线程执行完成
            cts.Cancel();  //结束线程池任务
            tasks = null; //标记清理线程池
            ResetState(); //重置状态
        }

        public void ResetAll()
        {
            if (queue == null || cts == null) return;

            cts.Cancel();
            queue.Clear();
            ResetState();
        }

        private void ResetState() => _ = Interlocked.Exchange(ref isProcessing, UnProcessing);
    }
}