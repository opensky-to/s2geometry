namespace OpenSky.S2Geometry.Datastructures
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class SortedMultiMapEnumerable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerator<KeyValuePair<TKey, TValue>>
    {
        MultiMap<TKey, TValue> map;
        IEnumerator<TKey> keyEnumerator;
        IEnumerator<TValue> valueEnumerator;


        public SortedMultiMapEnumerable(MultiMap<TKey, TValue> map)
        {
            this.map = map;
            this.Reset();
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        public KeyValuePair<TKey,TValue> Current
        {
            get
            {
                return new KeyValuePair<TKey, TValue>(this.keyEnumerator.Current, this.valueEnumerator.Current);
            }
        }


        public void Dispose()
        {
            this.keyEnumerator = null;
            this.valueEnumerator = null;
            this.map = null;
        }


        public bool MoveNext()
        {
            if (!this.valueEnumerator.MoveNext())
            {
                if (!this.keyEnumerator.MoveNext())
                    return false;
                this.valueEnumerator = this.map[this.keyEnumerator.Current].GetEnumerator();
                this.valueEnumerator.MoveNext();
                return true;
            }
            return true;
        }

        public void Reset()
        {
            this.keyEnumerator = this.map.Keys.OrderBy(k => k).GetEnumerator();
            this.valueEnumerator = new List<TValue>().GetEnumerator();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new SortedMultiMapEnumerable<TKey, TValue>(this.map);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
