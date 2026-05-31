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
    [SerializeField] private float _legSpeed;
    private float _jumpSpeed = 0.5f;

    [SerializeField] private float _legMaxBoundary;
    [SerializeField] private float _legOutwardMaxBoundary;

    [SerializeField] private float _legMinBoundary;
    [SerializeField] private float _legInwardMinBoundary;

    private float _angleBoundary = 40f;

    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = 0;
            legBase.lastGroundedPosition.transform.position = legBase.transform.position;
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
                offsetFromDefault = Mathf.DeltaAngle(leg.initialRotation.eulerAngles.y, RotateLegToFoot(leg, legBase));
            }

            bool snap = CheckForLegRotationSnap(leg, legBase, offsetFromDefault);


            // Moving block - I think it should be first but maybe I will move it later

            if (legBase.state != LegState.LockedToGround)
            {

                // Rotate base towards default 
                // Move base along direction

                MoveBaseBackForthAndRotate(leg, legBase);

                // Move base to ground

                MoveBaseToGround(leg, legBase);

                // Move foot to base

                Vector3 basePosition = new Vector3(legBase.transform.position.x, legBase.transform.position.y + 1.5f, legBase.transform.position.z);

                leg.MoveFootToPosition(basePosition);

            }
            else
            {
                MoveBaseToGround(leg, legBase);
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
                RotateLegToFoot(leg, legBase);
                MoveLegsUpDown(leg, legBase);
            }
            else if(_verticalVelocity > 1)
            {
                RotateLegToFoot(leg, legBase);
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

            legBase.lastGroundedPosition.transform.position = legBase.transform.position;
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

            legBase.lastGroundedPosition.transform.position = legBase.transform.position;
        }
    }


    private void MoveLegsUpDown(JointController leg, BaseController legBase)
    {
        if(legBase.transform.position.y >= legBase.lastGroundedPosition.transform.position.y)
        {
            legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y + verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
            leg.MoveFootToPosition(legBase.transform.position);
        }
    }


    private bool CheckIfLegFootBoundary(JointController leg, BaseController legBase)
    {
        Vector2 offset;

        if (leg.isStuckToGround)
        {
            offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x,
                leg.movePosition.transform.position.z - leg.centre.transform.position.z);
        }
        else
        {
            offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x,
                legBase.transform.position.z - leg.centre.transform.position.z);
        }

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

            Vector2 offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x,
                leg.movePosition.transform.position.z - leg.centre.transform.position.z);

            float distanceFromOrigin = offset.magnitude;

            if(distanceFromOrigin < (_legOutwardMaxBoundary + _legInwardMinBoundary) / 2)
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
        
        if(legsOffGroundCount > 4 && snap && legBase.state == LegState.LockedToGround)
        {
            legBase.tempState = LegState.LockedToGround;
            return;
        } 

        if (legsOffGroundCount < 5 && snap && legBase.state == LegState.LockedToGround)
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

        MoveBaseToGround(leg, legBase);

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


    public void MoveBaseToGround(JointController leg, BaseController legBase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        Vector3 raycastOrigin = new Vector3(legBase.transform.position.x, leg.centre.transform.position.y + 2, legBase.transform.position.z);

        if (Physics.Raycast(raycastOrigin, -Vector3.up, out hit, 10f, _groundedMask))
        {
            legBase.transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning($"Failed to snap {legBase.transform.name} to the ground");
        }
    }


    private float RotateLegToFoot(JointController leg, BaseController legBase)
    {
        float legAngle = leg.transform.localRotation.eulerAngles.y;

        Vector3 footDir = leg.movePosition.transform.position - leg.centre.transform.position;
        footDir = Vector3.ProjectOnPlane(footDir, Vector3.up).normalized;
        footDir = transform.InverseTransformDirection(footDir);

        Vector3 initialForward = Vector3.ProjectOnPlane(leg.initialRotation * Vector3.forward, Vector3.up);

        float footAngle = Vector3.SignedAngle(initialForward, footDir, Vector3.up);

        if (footAngle < 0)
        {
            footAngle = 360 + footAngle;
        }

        float offsetAngle = footAngle - legAngle;

        if (MathF.Abs(offsetAngle) > 1)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, legAngle + offsetAngle, 0f);
        }

        return legAngle;
    }


    private void RotateLegToBase(JointController leg, BaseController legBase)
    {
        float legAngle = leg.transform.localRotation.eulerAngles.y;

        Vector3 baseDir = legBase.transform.position - leg.centre.transform.position;
        baseDir = Vector3.ProjectOnPlane(baseDir, Vector3.up).normalized;
        baseDir = transform.InverseTransformDirection(baseDir);

        Vector3 initialForward = Vector3.ProjectOnPlane(leg.initialRotation * Vector3.forward, Vector3.up);

        float baseAngle = Vector3.SignedAngle(initialForward, baseDir, Vector3.up);

        if (baseAngle < 0)
        {
            baseAngle = 360 + baseAngle;
        }

        float offsetAngle = baseAngle - legAngle;

        if (MathF.Abs(offsetAngle) > 0.5)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, legAngle + offsetAngle, 0f);
        }

        return;
    }


    private void MoveBaseBackForthAndRotate(JointController leg, BaseController legBase)
    {
        // Gets offset of body to base (The destinatition it moves towards)
        Vector3 desiredPosition = legBase.lastGroundedPosition.transform.position;
        desiredPosition.y = legBase.transform.position.y;

        // Gets the position if it wasnt bound to an orbit
        desiredPosition = Vector3.MoveTowards(legBase.transform.position, desiredPosition, _legSpeed * Time.deltaTime * 10);

        // Binds to an orbit
        // Gets distance from base to centre in the x and y
        Vector2 offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x,
            legBase.transform.position.z - leg.centre.transform.position.z);
        float distanceFromOrigin = offset.magnitude;

        // Actually binds to orbit
        Vector3 direction = desiredPosition - leg.centre.transform.position;
        direction.y = 0f;
        direction.Normalize();

        desiredPosition = leg.centre.transform.position + direction * distanceFromOrigin;
        desiredPosition.y = legBase.transform.position.y;

        // Checks if we are close to target
        Vector3 currentDir = legBase.transform.position - leg.centre.transform.position;
        currentDir.y = 0f;
        currentDir.Normalize();

        Vector3 targetDir = legBase.lastGroundedPosition.transform.position - leg.centre.transform.position;
        targetDir.y = 0f;
        targetDir.Normalize();

        float angle = Vector3.Angle(currentDir, targetDir);

        if (angle > 1f)
        {
            // Updates position
            legBase.transform.position = desiredPosition;

            RotateLegToBase(leg, legBase);
        }
        
        // Moves back and forth
        Vector3 desiredDirection = (new Vector3(leg.centre.transform.position.x, legBase.transform.position.y, leg.centre.transform.position.z)
            - legBase.transform.position).normalized * legBase.direction;

        legBase.transform.position += desiredDirection * _legSpeed * 2f * Time.deltaTime;
    }
}
