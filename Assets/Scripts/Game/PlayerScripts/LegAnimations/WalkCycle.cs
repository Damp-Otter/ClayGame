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

    private float _verticalVelocity; public float verticalVelocity { get { return _verticalVelocity; } set { _verticalVelocity = value; } }
    private float _jumpHeight; public float jumpHeight { get { return _jumpHeight; } set { _jumpHeight = value; } }
    private float _legSpeed = 3f;
    private float _jumpSpeed = 0.5f;

    private float _legMaxBoundary = 2.5f;
    private float _legOutwardMaxBoundary = 2f;

    private float _legMinBoundary = 1f;
    private float _legInwardMinBoundary = 1.2f;

    private float _angleBoundary = 30f;

    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = 0;
            legBase.lastGroundedPosition = legBase.transform.position;
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

        if (characterGrounded)
        {
            // Rotate the leg so that it is inline with the foot

            float offsetFromDefault = 0f;

            if (legBase.state == LegState.LockedToGround)
            {
                offsetFromDefault = leg.initialRotation.eulerAngles.y - RotateLegToFoot(leg);
            }
            else
            {
                RotateLegToDefault(leg, legBase);
            }

            bool snap = CheckForLegRotationSnap(leg, legBase, offsetFromDefault);

            // Moving block - I think it should be first but maybe I will move it later

            if ((isMoving || isTurning) && characterGrounded)
            {
                if (legBase.state != LegState.LockedToGround)
                {
                    // Moving base

                    Vector3 currentDirection = (new Vector3(leg.centre.transform.position.x, legBase.transform.position.y, leg.centre.transform.position.z)
                        - legBase.transform.position).normalized * legBase.direction;
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

        }
        else
        {
            if (_verticalVelocity < -1)
            {
                RotateLegToFoot(leg);
                MoveLegsUpDown(leg, legBase);
            }
            else if(_verticalVelocity > 1)
            {
                RotateLegToFoot(leg);
                MoveLegsUpDown(leg, legBase);
            }
        }

        legBase.state = legBase.tempState;
    }

    public void HandleLanding()
    {

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(leg, legBase);

            SnapLegToGround(leg, legBase);

            legBase.state = LegState.LockedToGround;
            legBase.tempState = LegState.LockedToGround;
        }
    }


    public void HandleJumping()
    {
        foreach (var (leg, legBase) in _legsBases)
        {
            SnapLegToGround(leg, legBase);

            leg.isStuckToGround = false;
            legBase.state = LegState.LockedToGround;
            legBase.tempState = LegState.LockedToGround;
            legBase.lastGroundedPosition = legBase.transform.position;
        }
    }


    private void MoveLegsUpDown(JointController leg, BaseController legBase)
    {
        if(legBase.transform.position.y >= legBase.lastGroundedPosition.y)
        {
            legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y + verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
            leg.MoveFootToPosition(legBase.transform.position);
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


    private bool CheckForLegRotationSnap(JointController leg, BaseController legBase, float angleOffset)
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
        
        if(legsOffGroundCount > 2 && snap && legBase.state == LegState.LockedToGround)
        {
            legBase.tempState = LegState.LockedToGround;
            return;
        } 

        if (legsOffGroundCount < 3 && snap && legBase.state == LegState.LockedToGround)
        {

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
        }
    }


    private int GroundedLegsCount()
    {
        int groundedLegsCount = _legsBases.Count;

        foreach (var (leg, legBase) in _legsBases)
        {
            if (legBase.state != LegState.LockedToGround && legBase.tempState != LegState.LockedToGround)
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

        Vector3 desiredPosiiton = new Vector3(legBase.transform.position.x, leg.centre.transform.position.y - 0.7f, legBase.transform.position.z);

        leg.MoveFootToPosition(desiredPosiiton);
    }


    public void SnapLegToGround(JointController leg, BaseController legBase)
    {
        legBase.transform.position = leg.movePosition.transform.position;

        MoveBaseToGround(leg, legBase);

        leg.MoveFootToPosition(legBase.transform.position);

        leg.isStuckToGround = true;

        leg.LockCurrentPosition();
    }


    public void MoveBaseToGround(JointController leg, BaseController legbase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        Vector3 raycastOrigin = new Vector3(legbase.transform.position.x, leg.centre.transform.position.y, legbase.transform.position.z);

        Debug.DrawRay(raycastOrigin, -Vector3.up, Color.red, 4);

        if (Physics.Raycast(raycastOrigin, -Vector3.up, out hit, 5f, _groundedMask))
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
        float zLegAngle = leg.transform.localRotation.eulerAngles.y;

        Vector3 footDir = leg.movePosition.transform.position - leg.centre.transform.position;
        footDir = Vector3.ProjectOnPlane(footDir, Vector3.up).normalized;
        footDir = transform.InverseTransformDirection(footDir);

        Vector3 initialForward = Vector3.ProjectOnPlane(leg.initialRotation * Vector3.forward, Vector3.up);

        float footAngle = Vector3.SignedAngle(initialForward, footDir, Vector3.up);

        if (footAngle < 0)
        {
            footAngle = 360 + footAngle;
        }

        float offsetAngle = footAngle - zLegAngle;

        if (MathF.Abs(offsetAngle) > 1)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, zLegAngle + offsetAngle, 0f);
        }

        return zLegAngle;
    }


    private void RotateLegToDefault(JointController leg, BaseController legBase)
    {
        float legAngle = leg.transform.localRotation.eulerAngles.y;

        float defaultAngle = leg.initialRotation.eulerAngles.y;

        float offsetAngle = Mathf.DeltaAngle(legAngle, defaultAngle);

        if (offsetAngle > 3)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, legAngle + 3, 0f);
        }
        else if(offsetAngle < -3)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, legAngle - 3, 0f);
        }

        legBase.transform.position = leg.movePosition.transform.position;
    }
}
