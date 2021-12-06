namespace OpenSky.S2Geometry.Datastructures
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class MultiMap<TKey, TValue> : IMultiMap<TKey,TValue>, IDictionary<TKey,TValue>
    {
        private Dictionary<TKey, List<TValue>> interalStorage = new Dictionary<TKey, List<TValue>>();

        public MultiMap(){}

        public MultiMap(IEnumerable<KeyValuePair<TKey, TValue>> initialData)
        {
            foreach (var item in initialData)
            {
                this.Add(item);
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (!this.interalStorage.ContainsKey(key))
            {
                this.interalStorage.Add(key, new List<TValue>());
            }
            this.interalStorage[key].Add(value);
        }

        public void Add(TKey key, IEnumerable<TValue> valueList)
        {
            if (!this.interalStorage.ContainsKey(key))
            {
                this.interalStorage.Add(key, new List<TValue>());
            }
            foreach (TValue value in valueList)
            {
                this.interalStorage[key].Add(value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.interalStorage.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return this.interalStorage.Keys; }
        }

        public bool Remove(TKey key)
        {
            return this.interalStorage.Remove(key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            if (!this.interalStorage.ContainsKey(key))
            {
                value = default(TValue);
                return false;
            }
            value = this.interalStorage[key].Last();
            return true;
        }

        public ICollection<TValue> Values
        {
            get 
            { 
                List<TValue> retVal = new List<TValue>();
                foreach (var item in this.interalStorage)
                {
                    retVal.AddRange(item.Value);
                }
                return retVal;
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this.interalStorage[key].LastOrDefault();
            }
            set
            {
                this.Add(key,value);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (!this.interalStorage.ContainsKey(item.Key))
            {
                this.interalStorage.Add(item.Key, new List<TValue>());
            }
            this.interalStorage[item.Key].Add(item.Value);
        }

        public void Clear()
        {
            this.interalStorage.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            List<TValue> valueList;
            if (this.interalStorage.TryGetValue(item.Key, out valueList)) 
                return valueList.Contains(item.Value);
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var item in this.interalStorage)
            {
                foreach (TValue value in item.Value)
                {
                    array[i] = new KeyValuePair<TKey, TValue>(item.Key, value);
                    ++i;
                }
            }
        }

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var item in this.interalStorage)
                {
                    count += item.Value.Count;
                }
                return count;
            }
        }

        public bool CountIsAtLeast(int value)
        {
            int count = 0;
            foreach (var item in this.interalStorage)
            {
                count += item.Value.Count;
                if (count >= value)
                    return true;
            }
            return false;
        }

        int ICollection<KeyValuePair<TKey,TValue>>.Count
        {
	        get { return this.interalStorage.Count; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!this.ContainsKey(item.Key)) return false;

            var list = this.interalStorage[item.Key];
            var removed = list.Remove(item.Value);
            if (list.Count == 0)
                this.interalStorage.Remove(item.Key); // clear out the dict
            
            return removed;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new MultiMapEnumerator<TKey,TValue>(this);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>>  SortedValues
        {
            get { return new SortedMultiMapEnumerable<TKey, TValue>(this); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new MultiMapEnumerator<TKey, TValue>(this);
        }


        public List<TValue> this[TKey key]
        {
            get
            {
                if (!this.interalStorage.ContainsKey(key))
                    return new List<TValue>();
                return this.interalStorage[key];
            }
            set
            {
                if (!this.interalStorage.ContainsKey(key)) 
                    this.interalStorage.Add(key, value);
                else this.interalStorage[key] = value;
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            if (!this.ContainsKey(key)) return false;
            return this.interalStorage[key].Remove(value);
        }



        public bool Contains(TKey key, TValue item)
        {
           if (!this.interalStorage.ContainsKey(key)) return false;
           return this.interalStorage[key].Contains(item);
        }
    }
}
