using System;
using System.Diagnostics;

namespace ProceduralStructures {
    public class DebugStopwatch {
        private Stopwatch stopwatch;
        private string purpose;

        public DebugStopwatch() {
            stopwatch = new Stopwatch();
            purpose = "<unknown>";
        }

        public DebugStopwatch Start(string purpose) {
            this.purpose = purpose;
            stopwatch.Reset();
            stopwatch.Start();
            return this;
        }

        public DebugStopwatch Stop() {
            stopwatch.Stop();
            return this;
        }

        public override string ToString() {
            TimeSpan ts = stopwatch.Elapsed;
            return string.Format("{0}: {1:00}:{2:00}:{3:00}.{4:00}", purpose, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }
    }
}