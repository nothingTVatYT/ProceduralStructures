using System.Collections.Generic;

namespace ProceduralStructures {
    public class CircularReadonlyList<T> : List<T> {

        List<T> data;
        bool reversedAccess = false;
        public int indexOffset = 0;

        public CircularReadonlyList(List<T> orig) {
            data = orig;
        }

        public CircularReadonlyList(params T[] t) {
            data = new List<T>(t);
        }

        new public T this[int index] { get {
            int clippedIndex = index % data.Count;
            if (clippedIndex < 0) clippedIndex += data.Count;
            int realIndex = (data.Count + indexOffset + clippedIndex * (reversedAccess?-1:1)) % data.Count;
            return data[realIndex];
            }
        }

        new public int Count { get { return data.Count; } }

        new public void Reverse() {
            reversedAccess = !reversedAccess;
        }

        public void Shift() {
            if (!reversedAccess) indexOffset++; else indexOffset--;
            if (indexOffset < 0) indexOffset += data.Count;
        }
    }
}
