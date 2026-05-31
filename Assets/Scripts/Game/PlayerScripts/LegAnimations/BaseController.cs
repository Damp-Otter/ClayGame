using UnityEngine;

public class BaseController : MonoBehaviour
{

    public LegState state = LegState.Undetermined;
    public LegState tempState = LegState.Undetermined;
    public int direction = 1;

    public GameObject lastGroundedPosition;
}
