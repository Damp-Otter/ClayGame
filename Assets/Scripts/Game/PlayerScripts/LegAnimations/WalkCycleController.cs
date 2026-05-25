using System;
using System.Collections.Generic;
using UnityEngine;

/*
[Serializable]
public class LegBaseDictionaryItem
{
    [SerializeField] public JointController leg;
    [SerializeField] public BaseController legBase;
}


[Serializable]
public class LegBaseDictionary
{
    [SerializeField] LegBaseDictionaryItem[] thisDictItems;

    public Dictionary<JointController, BaseController> ToDictionary()
    {

        Dictionary<JointController, BaseController> thisDict = new Dictionary<JointController, BaseController>();

        foreach (var item in thisDictItems)
        {
            thisDict.Add(item.leg, item.legBase);
        }

        return thisDict;
    }
}


public class WalkCycleController : MonoBehaviour
{
    public enum LegState
    {
        MovingInwards,
        LockedToGround,
        MovingOutwards
    }

    private Dictionary<JointController, BaseController> _legsBases;
    private Dictionary<JointController, LegState> _legStates;
    private LegState _nextLegState;

    [SerializeField] LayerMask _groundedMask;
    [SerializeField] LegBaseDictionary serializedDict;

    public bool isMoving;
    public bool isJumping;
    public bool isTurning;
    public bool characterGrounded = false;
    public int turnDirection;

    //private float _baseMaxBoundary = 2f;
    //private float _baseMinBoundary = 1f;
    private float _legMaxBoundary = 2.5f;
    private float _legMinBoundary = 0.75f;

    private float _legSpeed = 4f;

    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();
        _legStates = new Dictionary<JointController, LegState>();

        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = 0;
            _legStates.Add(leg, LegState.LockedToGround);
        }


    }


    void Update()
    {
        foreach (var (leg, legBase) in _legsBases)
        {
            ControlLeg(leg, legBase);
        }
    }
    

    private void ControlLeg(JointController leg, BaseController legBase)
    {
        Vector3 basePosition = legBase.transform.position;

        if ((isMoving || isTurning) && characterGrounded)
        {
            if (isTurning)
            {
                // Rotating leg

                //Vector3 legRotationOffset = new Vector3(0, 0.1f * turnDirection, 0);
                //leg.TurnLegByOffset(legRotationOffset);
            }

            if (_legStates[leg] != LegState.LockedToGround)
            {
                // Moving base

                Vector3 currentDirection = leg.transform.up * legBase.direction;
                basePosition += currentDirection * _legSpeed * Time.deltaTime;

                // Moving leg

                Vector3 legPosition = new Vector3(basePosition.x, basePosition.y + 1f, basePosition.z);
                leg.MoveFootToPosition(legPosition);
            }
        }


        legBase.transform.position = basePosition;


        // Leg snapping up or down if needed

        bool snap = CheckIfLegPastBoundary(leg, legBase);

        LegSnapping(snap, leg, legBase);

        _legStates[leg] = _nextLegState;

        if (_legStates[leg] == LegState.LockedToGround)
        {
            leg.isStuckToGround = true;
        }
        else
        {
            leg.isStuckToGround = false;
        }
    }

    private bool CheckIfLegPastBoundary(JointController leg, BaseController legBase)
    {
        Vector2 offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x, leg.movePosition.transform.position.z - leg.centre.transform.position.z);

        float distanceFromOrigin = offset.magnitude;

        LegState currentState = _legStates[leg];

        bool snap = false;

        if (distanceFromOrigin > _legMaxBoundary)
        {
            if (currentState != LegState.MovingInwards)
            {
                snap = true;
            }

            if(currentState == LegState.MovingOutwards)
            {
                _nextLegState = LegState.LockedToGround;
            }
            else
            {
                _nextLegState = LegState.MovingInwards;
            }
        }
        else if (distanceFromOrigin < _legMinBoundary)
        {
            if (currentState != LegState.MovingOutwards)
            {
                snap = true;
            }

            if (currentState == LegState.MovingInwards)
            {
                _nextLegState = LegState.LockedToGround;
            }
            else
            {
                _nextLegState = LegState.MovingOutwards;
            }
        }

        return snap;
    }

    private void LegSnapping(bool snap, JointController leg, BaseController legBase)
    {
        int legsOffGroundCount = _legStates.Count - GroundedLegsCount();

        if (legsOffGroundCount < 5 && snap && _legStates[leg] == LegState.LockedToGround)
        {
            _legStates[leg] = _nextLegState;

            legBase.transform.position = leg.movePosition.transform.position;

            if (_legStates[leg] == LegState.MovingInwards)
            {
                legBase.direction = 1;
            }
            else if (_legStates[leg] == LegState.MovingOutwards)
            {
                legBase.direction = -1;
            }
            else
            {
                throw new Exception("State is lockedToGround but snapping.");
            }

            SnapLegOffGround(leg);
        }
        else if (snap)
        {
            _legStates[leg] = _nextLegState;

            legBase.direction = 0;
            SnapLegToGround(leg, legBase);
            legBase.transform.position = leg.movePosition.transform.position;
        }
    }

    private int GroundedLegsCount()
    {
        int groundedLegsCount = _legStates.Count;

        foreach (var (leg, legState) in _legStates)
        {
            if (_legStates[leg] != LegState.LockedToGround)
            {
                groundedLegsCount--;
            }
        }

        return groundedLegsCount;
    }

    public void HandleLanding()
    {

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(legBase);

            SnapLegToGround(leg, legBase);

            _legStates[leg] = LegState.LockedToGround;
        }

    }


    public void SnapLegToGround(JointController leg, BaseController legBase)
    {
        leg.MoveFootToPosition(legBase.transform.position);

        leg.isStuckToGround = true;
        _legStates[leg] = LegState.LockedToGround;

        leg.LockCurrentPosition();
    }


    public void SnapLegOffGround(JointController leg)
    {
        Vector3 offset = new Vector3(0, 1f, 0);

        leg.isStuckToGround = false;
        leg.MoveFootByOffset(offset);
    }


    public void MoveBaseToGround(BaseController legbase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        Vector3 raycastOrigin = new Vector3(legbase.transform.position.x, legbase.transform.position.y + 2f, legbase.transform.position.z);

        if (Physics.Raycast(raycastOrigin, -Vector3.up, out hit, 3f, _groundedMask))
        {
            legbase.transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning($"Failed to snap {legbase.transform.name} to the ground");
        }
    }

}
*/