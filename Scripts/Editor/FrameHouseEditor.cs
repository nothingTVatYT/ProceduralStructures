using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ExtensionMethods;

namespace ProceduralStructures {
[CustomEditor(typeof(FrameHouse))]
    public class FrameHouseEditor : Editor {

        public enum Command { SetX, SetY, SetZ }
        List<int> selectedIds = new List<int>();

        bool editFrame = false;
        bool captureDigits = false;
        string capturedDigits;
        Command previousCommand;
        FrameDefinition frame;

        public override void OnInspectorGUI() {
            FrameHouse house = target as FrameHouse;
            GUILayout.Label("Selected: " + selectedIds.Count + " (" + selectedIds.Elements() + ")");
            if (selectedIds.Count > 0) {
                GUILayout.Label("Selection Median: " + SelectionCenter());
            }
            editFrame = GUILayout.Toggle(editFrame, "Edit Frame");
            DrawDefaultInspector();
            if (GUILayout.Button("Build")) {
                ProceduralStructure ps = new ProceduralStructure();
                ps.ConstructFrameHouse(house, house.constructionRoot);
                EditorUtilities.CreateSecondaryUV(house.constructionRoot.GetComponentsInChildren<MeshFilter>());
            }
        }

        public void OnSceneGUI() {
            Event guiEvent = Event.current;
            FrameHouse house = target as FrameHouse;
            frame = house.frame;
            Transform tf = house.gameObject.transform;
            Handles.matrix = house.gameObject.transform.localToWorldMatrix;
            Vector3 size = new Vector3(0.1f, 0.1f, 0.1f);
            if (editFrame) {
                // handle keyboard events
                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                if (guiEvent.GetTypeForControl(controlID) == EventType.KeyDown) {
                    switch (guiEvent.keyCode) {
                        case KeyCode.D:
                        if (guiEvent.control) {
                            DuplicateSelectedPoints(frame);
                            guiEvent.Use();
                        } else if (guiEvent.shift) {
                            ExtrudeSelectedPoints(frame);
                            guiEvent.Use();
                        } else if (guiEvent.alt) {
                            RemoveSelectedPoints(frame);
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode.F:
                        if (guiEvent.alt) {
                            RemoveEdgeLoop(selectedIds);
                        } else {
                            CreateEdgeLoop(selectedIds);
                        }
                        guiEvent.Use();
                        break;
                        case KeyCode.S:
                        if (!guiEvent.shift && !guiEvent.control && !guiEvent.alt) {
                            SplitEdges(selectedIds);
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode.V:
                        if (!guiEvent.shift && !guiEvent.control && !guiEvent.alt) {
                            ValidateData();
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode.X:
                        if (!guiEvent.shift && !guiEvent.control && !guiEvent.alt) {
                            CaptureDigitsForCommand(Command.SetX);
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode.Y:
                        if (!guiEvent.shift && !guiEvent.control && !guiEvent.alt) {
                            CaptureDigitsForCommand(Command.SetY);
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode.Z:
                        if (!guiEvent.shift && !guiEvent.control && !guiEvent.alt) {
                            CaptureDigitsForCommand(Command.SetZ);
                            guiEvent.Use();
                        }
                        break;
                        case KeyCode digit when (digit >= KeyCode.Alpha0 && digit <= KeyCode.Alpha9) || digit == KeyCode.Period || digit == KeyCode.Backspace:
                        if (captureDigits) {
                            AddToNumber(guiEvent.keyCode, guiEvent.character);
                        }
                        guiEvent.Use();
                        break;
                        case KeyCode.Return:
                        StopCapturingDigits();
                        guiEvent.Use();
                        break;
                    }
                }
                // move selection
                if (selectedIds.Count > 0) {
                    Vector3 selectionCenter = SelectionCenter();
                    Vector3 newPos = Handles.PositionHandle(selectionCenter, Quaternion.identity);
                    if (newPos != selectionCenter) {
                        Vector3 diff = newPos - selectionCenter;
                        selectedIds.ForEach(i => frame.points[i] += diff);
                    }
                }
            }
            // draw edges
            if (frame.edges != null) {
                for (int i = 0; i < frame.edges.Count; i++) {
                    FrameDefinition.Edge edge = frame.edges[i];
                    if (edge.a < frame.points.Count && edge.b < frame.points.Count) {
                        Handles.DrawLine(frame.points[edge.a], frame.points[edge.b]);
                    }
                }
            }
            if (editFrame) {
                // draw handles for each point
                bool selectionChanged = false;
                for (int i = 0; i < frame.points.Count; i++) {
                    Vector3 p = frame.points[i];
                    Handles.color = (selectedIds.Contains(i)) ? Color.yellow : Color.white;
                    Handles.DrawWireCube(p, new Vector3(0.1f, 0.1f, 0.1f));
                    int controlId = GUIUtility.GetControlID(FocusType.Passive);
                    Vector3 np = Handles.FreeMoveHandle(controlId, p, Quaternion.identity, 0.1f, Vector3.zero, Handles.DotHandleCap);
                    if (controlId == EditorGUIUtility.hotControl) {
                        if (guiEvent.type == EventType.Used && guiEvent.button == 0) {
                            if (guiEvent.control) {
                                if (selectedIds.Contains(i)) {
                                    selectedIds.Remove(i);
                                } else {
                                    selectedIds.Add(i);
                                }
                            } else {
                                selectedIds.Clear();
                                selectedIds.Add(i);
                            }
                            selectionChanged = true;
                        }
                    }
                    //house.frame.points[i] = np;
                }
                if (selectionChanged) Repaint();
            }
        }

        void CaptureDigitsForCommand(Command command) {
            previousCommand = command;
            capturedDigits = "";
            captureDigits = true;
        }

        void StopCapturingDigits() {
            captureDigits = false;
            capturedDigits = "";
        }

        Vector3 SelectionCenter() {
            Vector3 selectionCenter = Vector3.zero;
            if (selectedIds.Count > 0) {
                selectedIds.ForEach(i => { if (i < frame.points.Count) { selectionCenter += frame.points[i]; }});
                selectionCenter/=selectedIds.Count;
            }
            return selectionCenter;
        }

        void AddToNumber(KeyCode code, char c) {
            if (code == KeyCode.Backspace) {
                if (capturedDigits.Length > 0) {
                    capturedDigits = capturedDigits.Substring(0, capturedDigits.Length-1);
                    if (capturedDigits == "") capturedDigits = "0";
                }
            } else {
                capturedDigits += c;
            }
            if (capturedDigits.Length > 0) {
                HandleNumber(previousCommand, float.Parse(capturedDigits), frame);
            }

        }

        void CreateEdgeLoop(List<int> vertexIndices) {
            for (int i = 0; i < vertexIndices.Count; i++) {
                int j = i+1;
                if (j >= vertexIndices.Count) j=0;
                FrameDefinition.Edge edge = new FrameDefinition.Edge(vertexIndices[i], vertexIndices[j]);
                if (edge.a != edge.b && !frame.edges.Contains(edge)) {
                    frame.edges.Add(edge);
                }
            }
        }

        void RemoveEdgeLoop(List<int> vertexIndices) {
            for (int i = 0; i < vertexIndices.Count; i++) {
                int j = i+1;
                if (j >= vertexIndices.Count) j=0;
                FrameDefinition.Edge edge = new FrameDefinition.Edge(vertexIndices[i], vertexIndices[j]);
                if (frame.edges.Contains(edge)) {
                    frame.edges.Remove(edge);
                }
            }
        }

        void SplitEdges(List<int> vertexIndices) {
            for (int i = 0; i < vertexIndices.Count; i++) {
                int j = i+1;
                if (j >= vertexIndices.Count) j = 0;
                FrameDefinition.Edge edge = new FrameDefinition.Edge(vertexIndices[i], vertexIndices[j]);
                int edgeIndex = frame.edges.IndexOf(edge);
                if (edgeIndex >= 0) {
                    Debug.Log("Split edge #" + edgeIndex + " " + frame.edges[edgeIndex]);
                    Vector3 m = (frame.points[edge.a] + frame.points[edge.b])/2;
                    frame.points.Add(m);
                    int newPointIndex = frame.points.Count-1;
                    edge.a = newPointIndex;
                    frame.edges[edgeIndex].b = newPointIndex;
                    frame.edges.Add(edge);
                    Debug.Log(" into #" + frame.edges[edgeIndex] + " " + frame.edges[edgeIndex] + " and #" + (frame.edges.Count-1) + " " + edge);
                }
            }
            ValidateData();
        }

        void HandleNumber(Command command, float number, FrameDefinition frame) {
            switch (command) {
                case Command.SetX:
                Vector3 selectionCenter = SelectionCenter();
                Vector3 newPos = new Vector3(number, selectionCenter.y, selectionCenter.z);
                if (newPos != selectionCenter) {
                    Vector3 diff = newPos - selectionCenter;
                    selectedIds.ForEach(i => frame.points[i] += diff);
                }
                break;
                case Command.SetY:
                selectionCenter = SelectionCenter();
                newPos = new Vector3(selectionCenter.x, number, selectionCenter.z);
                if (newPos != selectionCenter) {
                    Vector3 diff = newPos - selectionCenter;
                    selectedIds.ForEach(i => frame.points[i] += diff);
                }
                break;
                case Command.SetZ:
                selectionCenter = SelectionCenter();
                newPos = new Vector3(selectionCenter.x, selectionCenter.y, number);
                if (newPos != selectionCenter) {
                    Vector3 diff = newPos - selectionCenter;
                    selectedIds.ForEach(i => frame.points[i] += diff);
                }
                break;
            }
        }

        void RemoveSelectedPoints(FrameDefinition frame) {
            List<Vector3> vectors = new List<Vector3>();
            for (int i = 0; i < selectedIds.Count; i++) {
                frame.edges.RemoveAll(e => e.a == i || e.b == i);
            }
            DeletePoints(selectedIds);
            selectedIds.Clear();
        }

        void DuplicateSelectedPoints(FrameDefinition frame) {
            List<int> newPointIndices = new List<int>();
            for (int i = 0; i < selectedIds.Count; i++) {
                frame.points.Add(frame.points[selectedIds[i]]);
                newPointIndices.Add(frame.points.Count-1);
            }
            selectedIds = newPointIndices;
        }

        void ExtrudeSelectedPoints(FrameDefinition frame) {
            List<int> newPointIndices = new List<int>();
            for (int i = 0; i < selectedIds.Count; i++) {
                frame.points.Add(frame.points[selectedIds[i]]);
                frame.edges.Add(new FrameDefinition.Edge(selectedIds[i], frame.points.Count-1));
                newPointIndices.Add(frame.points.Count-1);
            }
            selectedIds = newPointIndices;
        }

        void ValidateData() {
            List<int> pointsToDelete = new List<int>();
            for (int i = 0; i < frame.points.Count-1; i++) {
                Vector3 v1 = frame.points[i];
                for (int j = frame.points.Count-1; j > i; j--) {
                    Vector3 v2 = frame.points[j];
                    if ((v2-v1).sqrMagnitude < 1e-3f) {
                        MergeVertex(i, j);
                        pointsToDelete.Add(j);
                    }
                }
            }
            if (pointsToDelete.Count > 0) {
                selectedIds.Clear();
            }
            DeletePoints(pointsToDelete);
        }

        void DeletePoints(List<int> pointsToDelete) {
            pointsToDelete.Sort();
            for (int i = pointsToDelete.Count-1; i >= 0; i--) {
                int pointIndex = pointsToDelete[i];
                for (int j = pointIndex+1; j < frame.points.Count; j++) {
                    ChangeIndex(j-1, j);
                }
                frame.points.RemoveAt(pointIndex);
            }
            for (int i = frame.edges.Count-1; i >= 0; i--) {
                if (frame.edges[i].a == frame.edges[i].b) {
                    frame.edges.RemoveAt(i);
                }
            }
            for (int i = frame.edges.Count-1; i >= 0; i--) {
                if (frame.edges[i].a >= frame.points.Count || frame.edges[i].b >= frame.points.Count) {
                    frame.edges.RemoveAt(i);
                }
            }
            for (int i = 0; i < frame.edges.Count; i++) {
                for (int j = frame.edges.Count-1; j > i; j--) {
                    if (frame.edges[j].Equals(frame.edges[i])) {
                        frame.edges.RemoveAt(j);
                    }
                }
            }
        }

        void MergeVertex(int to, int from) {
            foreach (FrameDefinition.Edge edge in frame.edges) {
                if (edge.a == from) edge.a = to;
                if (edge.b == from) edge.b = to;
            }
        }

        void ChangeIndex(int to, int from) {
            foreach (FrameDefinition.Edge edge in frame.edges) {
                if (edge.a == from) edge.a = to;
                if (edge.b == from) edge.b = to;
            }
        }
    }
}