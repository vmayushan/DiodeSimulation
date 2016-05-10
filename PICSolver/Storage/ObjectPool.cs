using System;
using System.Collections.Concurrent;

namespace PICSolver.Storage
{
    public class ObjectPool<T>
    {
        private readonly ConcurrentBag<T> objects;
        private readonly Func<T> objectGenerator;

        public ObjectPool(Func<T> objectGenerator)
        {
            if (objectGenerator == null) throw new ArgumentNullException(nameof(objectGenerator));
            objects = new ConcurrentBag<T>();
            this.objectGenerator = objectGenerator;
        }

        public T GetObject()
        {
            T item;
            if (objects.TryTake(out item)) return item;
            return objectGenerator();
        }

        public void PutObject(T item)
        {
            objects.Add(item);
        }
    }
}
