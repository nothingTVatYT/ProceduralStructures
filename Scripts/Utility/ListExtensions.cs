using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods {
    public static class ListExtensions {
        public static string Elements<T>(this List<T> list) {
            string result = "";
            foreach (T item in list) {
                if (result != "") result += ",";
                result += item.ToString();
            }
            return result;
        }

        public static string Elements<T>(this HashSet<T> list) {
            string result = "";
            foreach (T item in list) {
                if (result != "") result += ",";
                result += item.ToString();
            }
            return result;
        }

        public static T RandomItem<T>(this HashSet<T> set) {
            HashSet<T>.Enumerator enumerator = set.GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }
    }
}
