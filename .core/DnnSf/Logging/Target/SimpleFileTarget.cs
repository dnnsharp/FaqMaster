using DotNetNuke.Entities.Portals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace DnnSharp.FaqMaster.Core.DnnSf.Logging.Target
{
    public class SimpleFileTarget<T> : ILogTarget<T>, IDisposable
        where T : class
    {
        class FileLogOperation
        {
            public string FilePath { get; set; }
            public string Message { get; set; }
        }

        public const string EndLine = "\r\n";

        Func<T, string> FnGetFilePath;
        Func<eLogLevel, T, string, string> FnFormatMessage;

        ConcurrentQueue<FileLogOperation> Queue = new ConcurrentQueue<FileLogOperation>();

        public SimpleFileTarget(Func<T, string> fnGetFilePath, Func<eLogLevel, T, string, string> fnFormatMessage)
        {
            FnGetFilePath = fnGetFilePath;
            FnFormatMessage = fnFormatMessage;
        }

        static object _lockAquireLock = new object();
        static Dictionary<string, object> _lockWriteFile = new Dictionary<string, object>();

        public void Log(eLogLevel level,T data, string message)
        {
            // don't write directly, add to a queue and consume it on a different thread
            Queue.Enqueue(new FileLogOperation() {
                FilePath = FnGetFilePath(data),
                Message = FnFormatMessage(level, data, message)
            });

            ThreadPool.QueueUserWorkItem(ConsumeQueue);

            //new TaskFactory().StartNew(ConsumeQueue);
        }

        public void Log(eLogLevel level, T data, Exception ex)
        {
            // flatten the exception and log it as we do with any string
            Log(level, data, LoggerUtil.FlattenException(ex));
        }

        public void Dispose()
        {
            // try to flush the queue
            ConsumeQueue(null);
        }

        void ConsumeQueue(object state)
        {
            // only one writer at a time
            if (!Monitor.TryEnter(_lockAquireLock))
                return;

            try {
                FileLogOperation op = null;
                FileLogOperation firstFailed = null;
                while (Queue.TryDequeue(out op)) {

                    // check if we're back to the first op
                    if (firstFailed == op)
                        break;

                    try {
                        if (!Directory.Exists(Path.GetDirectoryName(op.FilePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(op.FilePath));
                        File.AppendAllText(op.FilePath, op.Message + EndLine);
                    } catch {
                        // save this in a variable so we know to break when we get to it again, we'll leave it for later
                        firstFailed = firstFailed ?? op;

                        // readd it to the queue
                        Queue.Enqueue(op);
                    }

                }
            } finally {
                Monitor.Exit(_lockAquireLock);
            }
        }

    }
}
