using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


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


public enum LegState
{
    MovingInwards,
    LockedToGround,
    MovingOutwards,
    InAir,
    Undetermined
}

public class WalkCycle : MonoBehaviour
{

    private Dictionary<JointController, BaseController> _legsBases;

    [SerializeField] LayerMask _groundedMask;
    [SerializeField] LegBaseDictionary serializedDict;

    public bool isMoving;
    public bool isJumping;
    public bool isTurning;
    public bool characterGrounded = false;
    public int turnDirection;

    private float _legSpeed = 4f;

    private float _legMaxBoundary = 2.5f;
    private float _legOutwardMaxBoundary = 1.8f;

    private float _legMinBoundary = 1f;
    private float _legInwardMinBoundary = 1.5f;

    private float _angleBoundary = 20f;

    //private LegState _tempLegState;


    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = 0;
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

        // Rotate the leg so that it is inline with the foot

        float offsetFromDefault = 0f;

        if (characterGrounded)
        {
            if (legBase.state == LegState.LockedToGround)
            {
                offsetFromDefault = RotateLegToFoot(leg);

                if(leg.transform.name == "Leg")
                {
                    Debug.Log($"Offset: {offsetFromDefault}");
                }
            }
            else
            {
                RotateLegToDefault(leg, legBase);
            }
        }

        bool snap = CheckForLegRotationSnap(leg, legBase, offsetFromDefault, leg.initialRotation.eulerAngles.y);

        // Moving block - I think it should be first but maybe I will move it later

        if ((isMoving || isTurning) && characterGrounded)
        {
            if (legBase.state != LegState.LockedToGround)
            {
                // Moving base

                Vector3 currentDirection = (leg.centre.transform.position - legBase.transform.position).normalized * legBase.direction;
                legBase.transform.position += currentDirection * _legSpeed * 2 * Time.deltaTime;

                Vector3 basePosition = legBase.transform.position;

                // Moving leg

                leg.MoveFootToPosition(legBase.transform.position);
            }
        }

        if (!snap)
        {
            snap = CheckIfLegFootBoundary(leg, legBase);
        }

        LegSnapping(snap, leg, legBase);

        legBase.state = legBase.tempState;
    }


    public void HandleLanding()
    {

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(legBase);

            SnapLegToGround(leg, legBase);

            legBase.state = LegState.LockedToGround;
            legBase.tempState = LegState.LockedToGround;
        }
    }


    private bool CheckIfLegFootBoundary(JointController leg, BaseController legBase)
    {
        Vector2 offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x, leg.movePosition.transform.position.z - leg.centre.transform.position.z);

        float distanceFromOrigin = offset.magnitude;

        bool snap = false;


        if (distanceFromOrigin > _legOutwardMaxBoundary && legBase.state == LegState.MovingOutwards)
        {
            snap = true;
            legBase.tempState = LegState.LockedToGround;
            return snap;
        }
        else if (distanceFromOrigin < _legInwardMinBoundary && legBase.state == LegState.MovingInwards)
        {
            snap = true;
            legBase.tempState = LegState.LockedToGround;
            return snap;
        }


        if (distanceFromOrigin > _legMaxBoundary && legBase.state != LegState.MovingInwards)
        {
            snap = true;
            legBase.tempState = LegState.MovingInwards;
        }
        else if (distanceFromOrigin < _legMinBoundary && legBase.state != LegState.MovingOutwards)
        {
            snap = true;
            legBase.tempState = LegState.MovingOutwards;
        }

        return snap;
    }


    private bool CheckForLegRotationSnap(JointController leg, BaseController legBase, float angleOffset, float defaultAngle)
    {
        bool snap = false;

        if (MathF.Abs(angleOffset) > _angleBoundary && legBase.state == LegState.LockedToGround)
        {
            snap = true;

            Vector2 offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x, leg.movePosition.transform.position.z - leg.centre.transform.position.z);

            float distanceFromOrigin = offset.magnitude;

            if(distanceFromOrigin < (_legMaxBoundary + _legMinBoundary) / 2)
            {
                legBase.tempState = LegState.MovingOutwards;
            }
            else
            {
                legBase.tempState = LegState.MovingInwards;
            }
        }


        return snap;
    }



    private void LegSnapping(bool snap, JointController leg, BaseController legBase)
    {
        int legsOffGroundCount = _legsBases.Count - GroundedLegsCount();

        if (snap && legBase.state == LegState.LockedToGround)
        {
            legBase.transform.position = leg.movePosition.transform.position;

            if (legBase.tempState == LegState.MovingInwards)
            {
                legBase.direction = 1;
            }
            else if (legBase.tempState == LegState.MovingOutwards)
            {
                legBase.direction = -1;
            }
            else
            {
                throw new Exception("State is lockedToGround but snapping.");
            }

            SnapLegOffGround(leg, legBase);
        }
        else if (snap)
        {
            SnapLegToGround(leg, legBase);

            legBase.transform.position = leg.movePosition.transform.position;
        }
    }

    private int GroundedLegsCount()
    {
        int groundedLegsCount = _legsBases.Count;

        foreach (var (leg, legBase) in _legsBases)
        {
            if (legBase.state != LegState.LockedToGround)
            {
                groundedLegsCount--;
            }
        }

        return groundedLegsCount;
    }


    public void SnapLegOffGround(JointController leg, BaseController legBase)
    {
        leg.isStuckToGround = false;

        legBase.transform.position = leg.movePosition.transform.position;

        Vector3 desiredPosiiton = new Vector3(legBase.transform.position.x, legBase.transform.position.y + 1f, legBase.transform.position.z);

        leg.MoveFootToPosition(desiredPosiiton);

        legBase.transform.position = leg.movePosition.transform.position;
    }


    public void SnapLegToGround(JointController leg, BaseController legBase)
    {
        legBase.transform.position = leg.movePosition.transform.position;

        MoveBaseToGround(legBase);

        leg.MoveFootToPosition(legBase.transform.position);

        leg.isStuckToGround = true;

        leg.LockCurrentPosition();
    }


    public void MoveBaseToGround(BaseController legbase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        Vector3 raycastOrigin = new Vector3(legbase.transform.position.x, legbase.transform.position.y + 1f, legbase.transform.position.z);

        Debug.DrawRay(raycastOrigin, -Vector3.up, Color.red);

        if (Physics.Raycast(raycastOrigin, -Vector3.up, out hit, 3f, _groundedMask))
        {
            legbase.transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning($"Failed to snap {legbase.transform.name} to the ground");
        }
    }


    private float RotateLegToFoot(JointController leg)
    {
        float zLegAngle = leg.transform.rotation.eulerAngles.y;

        Vector3 footDir = leg.movePosition.transform.position - leg.centre.transform.position;
        footDir = Vector3.ProjectOnPlane(footDir, Vector3.up).normalized;

        Vector3 initialForward = Vector3.ProjectOnPlane(leg.initialRotation * Vector3.forward, Vector3.up);

        float footAngle = Vector3.SignedAngle(initialForward, footDir, Vector3.up);

        if (footAngle < 0)
        {
            footAngle = 360 + footAngle;
        }

        float offsetAngle = footAngle - zLegAngle;

        if (MathF.Abs(offsetAngle) > 1)
        {
            leg.transform.rotation = Quaternion.Euler(90f, zLegAngle + offsetAngle, 0f);
        }

        return leg.initialRotation.eulerAngles.y - footAngle;
    }


    private void RotateLegToDefault(JointController leg, BaseController legBase)
    {
        float legAngle = leg.transform.rotation.eulerAngles.y;

        float defaultAngle = leg.initialRotation.eulerAngles.y;

        float offsetAngle = legAngle - defaultAngle;

        if (offsetAngle > 3)
        {
            leg.transform.rotation = Quaternion.Euler(90f, legAngle - 3, 0f);
        }
        else if(offsetAngle < -3)
        {
            leg.transform.rotation = Quaternion.Euler(90f, legAngle + 3, 0f);
        }


        legBase.transform.position = leg.movePosition.transform.position;
    }
}
