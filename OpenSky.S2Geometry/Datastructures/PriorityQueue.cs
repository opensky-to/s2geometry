namespace OpenSky.S2Geometry.Datastructures
{
    using System;
    using System.Collections.Generic;

    internal class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Enqueue(T item)
        {
            this.data.Add(item);
            int ci = this.data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (this.data[ci].CompareTo(this.data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = this.data[ci]; this.data[ci] = this.data[pi]; this.data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = this.data.Count - 1; // last index (before removal)
            T frontItem = this.data[0];   // fetch the front
            this.data[0] = this.data[li];
            this.data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && this.data[rc].CompareTo(this.data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (this.data[pi].CompareTo(this.data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = this.data[pi]; this.data[pi] = this.data[ci]; this.data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            T frontItem = this.data[0];
            return frontItem;
        }

        public int Count
        {
            get { return this.data.Count; }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < this.data.Count; ++i)
                s += this.data[i].ToString() + " ";
            s += "count = " + this.data.Count;
            return s;
        }

        public void Clear()
        {
            this.data.Clear();
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (this.data.Count == 0) return true;
            int li = this.data.Count - 1; // last index
            for (int pi = 0; pi < this.data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && this.data[pi].CompareTo(this.data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && this.data[pi].CompareTo(this.data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent
    } // PriorityQueue
}
