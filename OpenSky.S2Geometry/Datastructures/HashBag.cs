namespace OpenSky.S2Geometry.Datastructures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    // Adapted from https://github.com/sestoft/C5/blob/master/C5/hashing/HashBag.cs
    class HashBag<T> : ICollection<T>
    {
        readonly Dictionary<T, int> dict;
        int size;

        public HashBag() : this(EqualityComparer<T>.Default)
        {
        }

        public HashBag(IEqualityComparer<T> itemEqualityComparer)
        {
            this.dict = new Dictionary<T, int>(itemEqualityComparer);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in this.dict)
            {
                for (var i = 0; i < item.Value; i++)
                    yield return item.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Add(T item)
        {
            if (item == null)
            {
                return;
            }

            int val;
            if (this.dict.TryGetValue(item, out val))
            {
                this.dict[item] = ++val;
            }
            else
            {
                this.dict.Add(item, 1);
            }
            this.size++;
        }

        public void Clear()
        {
            this.dict.Clear();
            this.size = 0;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                return false;
            }

            return this.dict.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex < 0 || arrayIndex + this.Count > array.Length)
                throw new ArgumentOutOfRangeException();

            foreach (var p in this.dict)
                for (var j = 0; j < p.Value; j++)
                    array[arrayIndex++] = p.Key;
        }

        public bool Remove(T item)
        {
            if (item == null)
            {
                return false;
            }

            int val;

            if (this.dict.TryGetValue(item, out val))
            {
                this.size--;
                if (val == 1)
                    this.dict.Remove(item);
                else
                {
                    this.dict[item] = --val;
                }

                return true;
            }
            return false;
        }

        public int Count => this.size;
        public bool IsReadOnly => false;
    }
}
