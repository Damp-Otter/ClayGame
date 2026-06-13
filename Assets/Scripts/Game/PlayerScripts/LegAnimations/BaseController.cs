using UnityEngine;

public enum GroundedPositions
{
    Aligned,
    UnAligned,
    Clockwise,
    AntiClockwise
}

public class BaseController : MonoBehaviour
{

    public LegState state = LegState.Undetermined;
    public LegState tempState = LegState.Undetermined;
    public int direction = 1;

    public GameObject lastGroundedPosition;
    public GameObject trueGroundedPosition;
    public GroundedPositions groundedPositions = GroundedPositions.Aligned;
    public float angleBoundary = 45f;
}
