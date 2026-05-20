using UnityEngine;

public class BaseController : MonoBehaviour
{

    public BaseState state = BaseState.Moving;
    public int direction = 1;

    public enum BaseState
    {
        Min,
        Moving,
        Max
    }

    public void SetState(int stateIndex)
    {
        switch (stateIndex)
        {
            case 1:
                state = BaseState.Min;
                break;
            case 2:
                state = BaseState.Moving;
                break;
            case 3:
                state = BaseState.Max;
                break;
            default:
                break;
        }
    }
}
