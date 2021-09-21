using System.Collections.Generic;

namespace ProceduralStructures {
    public class CircularReadonlyList<T> : List<T> {

        List<T> data;
        bool reversedAccess = false;
        public int indexOffset = 0;

        ///<summary>This creates a read-only view on the list which will not be changed</summary>
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

        /// <summary>This just changes the access order, it won't change the underlying list</summary>
        new public void Reverse() {
            reversedAccess = !reversedAccess;
        }

        ///<summary>Shift and rotate the items so that the new index 0 points to the previous 1 and so on
        public void Shift() {
            if (!reversedAccess) indexOffset++; else indexOffset--;
            if (indexOffset < 0) indexOffset += data.Count;
        }
    }
}
