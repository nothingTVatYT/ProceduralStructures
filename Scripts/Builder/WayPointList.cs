using System;
using System.Collections.Generic;

namespace ProceduralStructures {
    [Serializable]
    public class WayPointList {
        public string name;
        public List<WayPoint> wayPoints;
        public WayPointList(string name, List<WayPoint> points) {
            this.name = name;
            this.wayPoints = points;
        }
        public int Count { get { return wayPoints != null ? wayPoints.Count : 0; } }
    }
}