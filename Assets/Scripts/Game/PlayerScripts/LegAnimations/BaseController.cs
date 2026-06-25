using UnityEngine;

public enum GroundedPositions
{
    Aligned,
    UnAligned,
    Clockwise,
    AntiClockwise
}

public enum RotationTarget
{
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
    public RotationTarget rotationTarget;
    public GroundedPositions groundedPositions = GroundedPositions.Aligned;
    public bool exceededFinalBoundary = false;
    public float initialRotationOffset = 0;
    public float targetRotationOffset = 0;
    public float initialOffset = 0;
    public float angleBoundary = 50;

    public BaseController leftBase;
    public BaseController rightBase;
}