using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DnnSharp.FaqMaster.Core.DnnSf
{
    public class ConcurrentQueue<T>
    {
        private readonly Queue<T> queue = new Queue<T>();
        public ConcurrentQueue() 
        { 
        }

        public void Enqueue(T item)
        {
            lock (queue) {
                queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            lock (queue) {

                if (queue.Count == 0)
                    return default(T);

                return queue.Dequeue();
            }
        }

        bool closing;
        public void Close()
        {
            lock (queue) {
                closing = true;
            }
        }

        public bool TryDequeue(out T value)
        {
            lock (queue) {
                if (queue.Count == 0 || closing) {
                    value = default(T);
                    return false;
                }
                
                value = queue.Dequeue();
                return true;
            }
        }
    }
}
