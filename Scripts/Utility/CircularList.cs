using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class CircularList<T> : IEnumerable<T> {
        public static int NOT_FOUND = int.MinValue;
        public readonly int NotFound = NOT_FOUND;
        List<T> data;
        int indexOffset = 0;
        bool reversedAccess = false;

        public int IndexOffset {
            get { return indexOffset; }
            set { indexOffset = value; }
        }

        public int Count {
            get { return data != null ? data.Count : 0; }
        }

        public CircularList(IEnumerable<T> l) {
            data = new List<T>(l);
        }
        public void Reverse() {
            reversedAccess = !reversedAccess;
        }

        public IEnumerator<T> GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return (IEnumerator) GetEnumerator();
        }

        public T this[int index] {
            get { return data[ToDataIndex(index)]; }
            set { data[ToDataIndex(index)] = value; }
        }

        public bool Contains(T item) {
            return data.Contains(item);
        }

        public int IndexOf(T item) {
            int dataIndex = data.IndexOf(item);
            if (dataIndex < 0) return NOT_FOUND;
            return ToVirtualIndex(dataIndex);
        }

        public bool IsConsecutiveIndex(int i1, int i2) {
            return i2 == (i1+1)%data.Count;
        }

        public bool Remove(T item) {
            return data.Remove(item);
        }

        public void RemoveAt(int index) {
            data.RemoveAt(ToDataIndex(index));
        }

        int ToDataIndex(int index) {
            int clippedIndex = index % data.Count;
            if (clippedIndex < 0) clippedIndex += data.Count;
            return (data.Count + indexOffset + clippedIndex * (reversedAccess?-1:1)) % data.Count;
        }

        int ToVirtualIndex(int index) {
            return reversedAccess ? (data.Count - index - indexOffset)%data.Count : index - indexOffset;
        }

        public class Enumerator : IEnumerator<T> {
            CircularList<T> c;
            int index = -1;
            public Enumerator(CircularList<T> l) {
                this.c = l;
            }
            public T Current {
                get { return c[index]; }
            }

            object IEnumerator.Current {
                    get { return Current; }
            }

            public bool MoveNext() {
                if (index < c.Count) {
                    index++;
                    return true;
                }
                return false;
            }

            public void Reset() {
                index = -1;
            }

            public void Dispose() {}
        }
    }
}
