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
    }
}
