using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PipeWebSocket
{
    public class ConcurrentAsyncQueue<T>
    {
        private readonly ConcurrentQueue<T> queue = null;
        public Func<T, Task> OnProcess = null;
        public Action<T, Exception> OnException = null;
        private volatile bool isProcessing = false;   //队列是否正在处理数据
        public int ThreadCount { get; }  //消费线程数量

        public ConcurrentAsyncQueue(int threadCount = 1)
        {
            ThreadCount = threadCount;
            queue = new ConcurrentQueue<T>();
        }

        public int Count { get => queue.Count; }

        public void Push(T obj)
        {
            queue.Enqueue(obj);
            if (!isProcessing) CreateTasks();
        }

        private async Task ListeninAsync()
        {
            while (queue.Count > 0 && isProcessing)
            {
                if (queue.TryDequeue(out T item))
                {
                    try
                    {
                        await OnProcess?.Invoke(item);
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
            try
            {
                isProcessing = true;
                Task[] tasks = new Task[ThreadCount];
                for (int i = 0; i < ThreadCount; i++)
                {
                    var task = Task.Factory.StartNew(async () => await ListeninAsync(), TaskCreationOptions.LongRunning);
                    tasks[i] = task;
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);  //等待消费线程执行完成
            }
            catch (Exception ex)
            {
                OnException?.Invoke(default, ex);
            }
            finally
            {
                ResetState(); //重置状态
            }
        }

        public void ResetAll()
        {
            if (queue == null) return;

            queue.Clear();
            ResetState();
        }

        private void ResetState() => isProcessing = false;
    }
}