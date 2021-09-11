using UnityEngine;

namespace ProceduralStructures {
    public class CityMarker : MonoBehaviour
    {
        public CityDefinition cityDefinition;

        public void OnDrawGizmosSelected() {
            foreach (CityDefinition.Street street in cityDefinition.streets) {
                if (street.points.Count >= 2) {
                    for (int i = 1; i < street.points.Count; i++) {
                        Gizmos.DrawLine(street.points[i-1], street.points[i]);
                    }
                }
            }
        }
    }
}