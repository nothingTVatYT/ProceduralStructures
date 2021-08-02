using UnityEngine;

[CreateAssetMenu(fileName = "LadderDefinition", menuName = "Procedural Structures/Ladder Definition", order = 1)]
public class LadderDefinition : ScriptableObject
{
    public float stepWidth;
    public float stepHeight;
    public float stepThickness;
    public float poleThickness;
    public int steps;
    public Material ladderMaterial;
    public float uvScale;
    public float TotalHeight { get { return stepHeight * (steps+2); }}
}
