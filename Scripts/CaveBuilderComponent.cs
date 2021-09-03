using System.Collections.Generic;
using UnityEngine;

namespace ProceduralStructures {
    public class CaveBuilderComponent : MonoBehaviour
    {
        public GameObject generatedMeshParent;

        public bool useTransforms;
        public Transform wayPointTransforms;
        public CaveDefinition caveDefinition;

        void Start()
        {
            UpdateWayPoints();
        }

        public void UpdateWayPoints() {
            List<WayPointList> lists = new List<WayPointList>();
            for (int i = 0; i < wayPointTransforms.childCount; i++) {
                List<WayPoint> points = new List<WayPoint>();
                Transform tunnel = wayPointTransforms.GetChild(i);
                for (int j = 0; j < tunnel.childCount; j++) {
                    Transform tf = tunnel.GetChild(j);
                    WayPoint wp = new WayPoint(tf.position, tf.localScale.x, tf.localScale.y);
                    wp.name = tf.gameObject.name;
                    points.Add(wp);
                }
                lists.Add(new WayPointList(tunnel.gameObject.name, points));
            }
            caveDefinition.wayPointLists = lists;
        }

        public void OnDrawGizmosSelected() {
            if (caveDefinition.IsValid()) {
                foreach (WayPointList list in caveDefinition.wayPointLists) {
                    Vector3 a = list.wayPoints[0].position;
                    int n = 0;
                    Gizmos.color = Color.yellow;
                    foreach (Vector3 v in caveDefinition.GetVertices(list)) {
                        n++;
                        Gizmos.DrawLine(a, v);
                        Gizmos.DrawSphere(v, 0.1f);
                        a = v;
                    }
                }
            }
        }
    }
}