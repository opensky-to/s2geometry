namespace OpenSky.S2Geometry.Datastructures
{
    using System.Collections;
    using System.Collections.Generic;

    internal class MultiMapEnumerator<TKey,TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        MultiMap<TKey,TValue> map;
        IEnumerator<TKey> keyEnumerator;
        IEnumerator<TValue> valueEnumerator;

        public MultiMapEnumerator(MultiMap<TKey,TValue> map)
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
            this.keyEnumerator = this.map.Keys.GetEnumerator();
            this.valueEnumerator = new List<TValue>().GetEnumerator();
        }
    }
}
