using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Logger;

namespace Kontur.ImageTransformer
{
    internal class AsyncHttpServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly uint _maxTasksNumber;

        private int _counter;

        private bool _disposed;

        private volatile bool _isRunning;

        private Thread _listenerThread;

        public AsyncHttpServer(uint maxTasksNumber)
        {
            _maxTasksNumber = maxTasksNumber;

            _listener = new HttpListener();
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncHttpServer), "server stoped");

            _disposed = true;

            Stop();

            _listener.Close();
        }

        public void Start(int port, string prefix)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncHttpServer), "server stoped");

            lock (_listener)
            {
                if (_isRunning) return;

                _listener.Prefixes.Clear();
                _listener.Prefixes.Add(string.Format($"http://+:{port}/{prefix}/"));
                _listener.Start();

                _listenerThread = new Thread(HandleRequests)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Highest
                };
                _listenerThread.Start();

                _isRunning = true;
            }
        }

        public void Stop()
        {
            lock (_listener)
            {
                if (!_isRunning)
                    return;

                _listener.Stop();

                _listenerThread.Abort();
                _listenerThread.Join();

                _isRunning = false;
            }
        }

        private void HandleRequests()
        {
            while (_listener.IsListening)
                try
                {
                    var context = _listener.BeginGetContext(ContextReceptionCallbackAsync, _listener);
                    context.AsyncWaitHandle.WaitOne();
                }
                catch (Exception error)
                {
                    LoggerWrapper.LoggError("AsyncHttpServer:", error);
                }
        }

        private async void ContextReceptionCallbackAsync(IAsyncResult arasyncResult)
        {
            try
            {
                var context = _listener.EndGetContext(arasyncResult);

                if (_counter <= _maxTasksNumber)
                {
                    Interlocked.Increment(ref _counter);
                    await HandleContextAsync(context).ContinueWith(o => Interlocked.Decrement(ref _counter));
                }
                else
                {
                    SendEmptyResponse(context, 429);
                }
            }
            catch (ObjectDisposedException error)
            {
                LoggerWrapper.LoggWarning("AsyncHttpServer:", error);
            }
            catch (Exception error)
            {
                LoggerWrapper.LoggError("AsyncHttpServer:", error);
            }
        }

        protected virtual async Task HandleContextAsync(HttpListenerContext listenerContext)
        {
            await Task.Run(() => SendEmptyResponse(listenerContext, (int) HttpStatusCode.OK)).ConfigureAwait(false);
        }

        protected void SendEmptyResponse(HttpListenerContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Close();
        }
    }
}