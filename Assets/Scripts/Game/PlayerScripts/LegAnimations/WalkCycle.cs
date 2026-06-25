using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;


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

    [SerializeField] private Transform _parentTransform;
    public bool isMoving;
    public bool isJumping;
    public bool isTurning;
    public bool characterGrounded = false;
    public int turnDirection;

    private float _verticalVelocity; public float verticalVelocity { get { return _verticalVelocity; } set { _verticalVelocity = value; } }
    private float _jumpHeight; public float jumpHeight { get { return _jumpHeight; } set { _jumpHeight = value; } }
    [SerializeField] private float _legSpeed;
    [SerializeField] private float _legRotateSpeed;
    [SerializeField] private float _speedScale;
    private float _jumpSpeed = 0.5f;
    private float _arcHeight = 4;
    private float _arcOffset;
    private bool _jumping;
    private float _jumpLegMinOffset = 0.5f;
    private float _jumpLegMaxOffset = 1.5f;
    private float _jumpLegBoundary = 0.1f;


    [SerializeField] private float _legMaxBoundary;
    [SerializeField] private float _legOutwardMaxBoundary;

    [SerializeField] private float _legMinBoundary;
    [SerializeField] private float _legInwardMinBoundary;

    [SerializeField] private float _offsetBeyondRotation;
    [SerializeField] private float _offsetBeyondRotationUnAligned;


    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        ChangeToScale();


        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = 0;
            HandleGroundPointAligned(leg, legBase);
            HandleJumpTriggered();
        }
    }

    void Update()
    {
        foreach (var (leg, legBase) in _legsBases)
        {
            ControlLeg(leg, legBase);
        }
    }

    private void ChangeToScale()
    {
        float scale = _parentTransform.localScale.x;
        _legMaxBoundary *= scale;
        _legMinBoundary *= scale;
        _legOutwardMaxBoundary *= scale;
        _legInwardMinBoundary *= scale;
        _arcHeight *= scale;
        _legSpeed *= scale;
        _jumpLegMaxOffset *= scale;
        _jumpLegMinOffset *= scale;
        _jumpLegBoundary *= scale;

        _legSpeed *= _speedScale;
        _jumpSpeed *= _speedScale;
    }


    private void ControlLeg(JointController leg, BaseController legBase)
    {
        float offsetFromOrigin = GetOffsetFromOrigin(leg, legBase);

        if (characterGrounded)
        {
            // Rotate the leg so that it is inline with the foot

            float offsetFromDefault = 0f;

            // Move base to ground

            MoveBaseToGround(leg, legBase);

            offsetFromDefault = Mathf.DeltaAngle(leg.defaultRotation.eulerAngles.y, leg.transform.localRotation.eulerAngles.y);
            bool snap = false;

            if (legBase.state == LegState.LockedToGround)
            {
                RotateLegToFoot(leg, legBase);

                snap = CheckForLegRotationSnap(leg, legBase, offsetFromDefault);

                SafeMoveFootToPosition(leg, legBase, leg.lockedGroundPosition);
            }

            // Moving block - I think it should be first but maybe I will move it later

            if (legBase.state != LegState.LockedToGround)
            {
                // Rotate base towards default 
                // Move base along direction

                MoveBaseBackForthAndRotate(leg, legBase, offsetFromDefault);

                // Move foot to base

                float arc = CalculateArcingOffset(leg, legBase, offsetFromOrigin);

                Vector3 basePosition = new Vector3(legBase.transform.position.x, legBase.transform.position.y + arc, legBase.transform.position.z);

                SafeMoveFootToPosition(leg, legBase, basePosition);
            }

            // Handle ledges and the grounded points

            HandleGroundedPositions(leg, legBase);

            snap = CheckIfLegFootBoundary(leg, legBase) || snap;

            LegSnapping(snap, leg, legBase);

        }
        else
        {
            if (_jumping)
            {
                MoveTowardsDefault(leg, legBase, offsetFromOrigin);

                if (Mathf.Abs(_verticalVelocity) > 2)
                {
                    RotateLegToFoot(leg, legBase);
                    MoveLegsUpDown(leg, legBase);
                }
            }
            else
            {
                HandleJumpTriggered();
            }
        }

        legBase.state = legBase.tempState;
    }


    private float CalculateArcingOffset(JointController leg, BaseController legBase, float offsetFromDefault)
    {
        float percentThroughBoundaries = 0;

        if (legBase.state == LegState.MovingOutwards)
        {
            percentThroughBoundaries = (offsetFromDefault - _legMinBoundary) / (_legOutwardMaxBoundary - _legMinBoundary);
        }
        else if (legBase.state == LegState.MovingInwards)
        {
            percentThroughBoundaries = (offsetFromDefault - _legInwardMinBoundary) / (_legMaxBoundary - _legInwardMinBoundary);
        }
        /*
        if (legBase.state == LegState.MovingOutwards)
        {
            percentThroughBoundaries = (offsetFromDefault - legBase.initialOffset) / (_legOutwardMaxBoundary - legBase.initialOffset);
        }
        else if (legBase.state == LegState.MovingInwards)
        {
            percentThroughBoundaries = (offsetFromDefault - _legInwardMinBoundary) / (legBase.initialOffset - _legInwardMinBoundary);
        }
        */
        float arc = (0.5f - Mathf.Abs(percentThroughBoundaries - 0.5f)) * _arcHeight;

        if (leg.transform.name == "Leg (2)")
        {
            //Debug.Log($"Arc: {arc}, Percent: {percentThroughBoundaries}, Initial offset: {legBase.initialOffset}");
        }

        return arc;
    }


    private void HandleGroundedPositions(JointController leg, BaseController legBase)
    {
        bool trueGroundPointGrounded = TryGetGroundPoint(legBase.trueGroundedPosition.transform.position, 1.5f) != Vector3.zero;

        if (!trueGroundPointGrounded)
        {
            RotateGroundedPosition(leg, legBase);
        }

        if((trueGroundPointGrounded && legBase.groundedPositions != GroundedPositions.Aligned) || legBase.groundedPositions == GroundedPositions.Aligned)
        {
            HandleGroundPointAligned(leg, legBase);
        }
    }


    public void HandleLanding()
    {
        characterGrounded = true;

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(leg, legBase);

            SnapLegToGround(leg, legBase);

            legBase.state = LegState.LockedToGround;
            legBase.tempState = LegState.LockedToGround;

            legBase.lastGroundedPosition.transform.position = legBase.transform.position;

            legBase.groundedPositions = GroundedPositions.Aligned;
        }

        _jumping = false;
    }


    public void HandleJumpTriggered()
    {
        characterGrounded = false;

        foreach (var (leg, legBase) in _legsBases)
        {
            SnapLegToGround(leg, legBase);

            leg.isStuckToGround = false;
            legBase.state = LegState.LockedToGround;
            legBase.tempState = LegState.LockedToGround;

            // Determine direction
            DetermineLegDirectionOnJump(leg, legBase);

            legBase.lastGroundedPosition.transform.position = legBase.trueGroundedPosition.transform.position;
        }

        _jumping = true;

    }


    private void MoveLegsUpDown(JointController leg, BaseController legBase)
    {
        // Moving legs up 

        if(_verticalVelocity > 0)
        {
            if (legBase.transform.position.y <= legBase.trueGroundedPosition.transform.position.y + _jumpLegMaxOffset)
            {
                legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y + verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
                SafeMoveFootToPosition(leg, legBase, legBase.transform.position);
            }
            else
            {
                legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y - verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
                SafeMoveFootToPosition(leg, legBase, legBase.transform.position);
            }
        }

        // Moving legs down

        else
        {
            if (legBase.transform.position.y >= legBase.trueGroundedPosition.transform.position.y - _jumpLegMinOffset)
            {
                legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y + verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
                SafeMoveFootToPosition(leg, legBase, legBase.transform.position);
            }
            else
            {
                legBase.transform.position = new Vector3(legBase.transform.position.x, legBase.transform.position.y - verticalVelocity * _jumpSpeed * Time.deltaTime, legBase.transform.position.z);
                SafeMoveFootToPosition(leg, legBase, legBase.transform.position);
            }
        }
    }


    private void DetermineLegDirectionOnJump(JointController leg, BaseController legBase)
    {
        float offsetFromOrigin = GetOffsetFromOrigin(leg, legBase);

        float targetOffset = new Vector2(legBase.trueGroundedPosition.transform.position.x - leg.centre.transform.position.x,
            legBase.trueGroundedPosition.transform.position.z - leg.centre.transform.position.z).magnitude;

        if (offsetFromOrigin > targetOffset)
        {
            legBase.tempState = LegState.MovingInwards;
            legBase.state = LegState.MovingInwards;

            legBase.direction = 1; // Change these to 1 and -1
        }
        else
        {
            legBase.tempState = LegState.MovingOutwards;
            legBase.state = LegState.MovingOutwards;

            legBase.direction = -1;
        }
    }


    private void MoveTowardsDefault(JointController leg, BaseController legBase, float offsetFromOrigin)
    {
        // Gets offset of body to base (The destinatition it moves towards)
        Vector3 desiredPosition = legBase.lastGroundedPosition.transform.position;
        desiredPosition.y = legBase.transform.position.y;

        // Gets the position if it wasnt bound to an orbit
        desiredPosition = Vector3.MoveTowards(legBase.transform.position, desiredPosition, _legSpeed * Time.deltaTime * 3);

        // Binds to an orbit
        // Gets distance from base to centre in the x and y
        Vector2 offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x,
            legBase.transform.position.z - leg.centre.transform.position.z);
        float distanceFromOrigin = offsetFromOrigin;

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

        float targetOffset = new Vector2(legBase.trueGroundedPosition.transform.position.x - leg.centre.transform.position.x,
            legBase.trueGroundedPosition.transform.position.z - leg.centre.transform.position.z).magnitude;

        // Moves back and forth if far from the trueGroundedPoint
        if (Mathf.Abs(offsetFromOrigin - targetOffset) > _jumpLegBoundary && legBase.state == LegState.InAir)
        {
            Vector3 desiredDirection = (new Vector3(leg.centre.transform.position.x, legBase.transform.position.y, leg.centre.transform.position.z)
                - legBase.transform.position).normalized * legBase.direction;

            legBase.transform.position += desiredDirection * _legSpeed * Time.deltaTime;
        }
        else
        {
            legBase.tempState = LegState.InAir;
            legBase.state = LegState.InAir;
        }

        leg.movePosition.transform.position = legBase.transform.position;
    }


    private float GetOffsetFromOrigin(JointController leg, BaseController legBase)
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

        return offset.magnitude;
    }


    private bool CheckIfLegFootBoundary(JointController leg, BaseController legBase)
    {
        float offset = GetOffsetFromOrigin(leg, legBase);

        bool snap = false;

        bool rotationComplete = false;
        float angleToDestination = GetAngleToLastPosition(leg, legBase);

        if(legBase.rotationTarget == RotationTarget.Clockwise && angleToDestination < legBase.targetRotationOffset)
        {
            rotationComplete = true;
        }
        else if(legBase.rotationTarget == RotationTarget.AntiClockwise && angleToDestination > legBase.targetRotationOffset)
        {
            rotationComplete = true;
        }

        if(leg.transform.name == "Leg (2)")
        {
            Debug.Log($"Angle remaining {angleToDestination}, Target {legBase.targetRotationOffset}, rotation complete {rotationComplete}, rotation {legBase.rotationTarget}");
        }

        if (offset > _legOutwardMaxBoundary && legBase.state == LegState.MovingOutwards)
        {
            snap = true;
            legBase.tempState = LegState.LockedToGround;
            return snap;
        }
        else if (offset < _legInwardMinBoundary && legBase.state == LegState.MovingInwards)
        {
            snap = true;
            legBase.tempState = LegState.LockedToGround;
            return snap;
        }

        if (offset > _legMaxBoundary && legBase.state != LegState.MovingInwards)
        {
            snap = true;
            legBase.tempState = LegState.MovingInwards;
        }
        else if (offset < _legMinBoundary && legBase.state != LegState.MovingOutwards)
        {
            snap = true;
            legBase.tempState = LegState.MovingOutwards;
        }

        return snap;
    }


    private bool CheckForLegRotationSnap(JointController leg, BaseController legBase, float angleOffset)
    {
        bool snap = false;

        if (MathF.Abs(angleOffset) > legBase.angleBoundary && legBase.state == LegState.LockedToGround)
        {
            snap = true;

            Vector2 offset = new Vector2(leg.movePosition.transform.position.x - leg.centre.transform.position.x,
                leg.movePosition.transform.position.z - leg.centre.transform.position.z);

            float distanceFromOrigin = offset.magnitude;

            CalculateLegState(leg, legBase);
        }

        float angleToDefault = GetAngleToLastPosition(leg, legBase);

        return snap;
    }


    private void CalculateLegState(JointController leg, BaseController legBase)
    {
        float distanceFromOrigin = GetOffsetFromOrigin(leg, legBase);

        if (distanceFromOrigin < (_legOutwardMaxBoundary + _legInwardMinBoundary) / 2)
        {
            legBase.tempState = LegState.MovingOutwards;
        }
        else
        {
            legBase.tempState = LegState.MovingInwards;
        }
    }


    private void LegSnapping(bool snap, JointController leg, BaseController legBase)
    {
        int legsOffGroundCount = _legsBases.Count - GroundedLegsCount();
        
        if(legsOffGroundCount > 3 && snap && legBase.state == LegState.LockedToGround)
        {
            legBase.tempState = LegState.LockedToGround;
            return;
        } 

        if (legsOffGroundCount < 4 && snap && legBase.state == LegState.LockedToGround)
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
        // Make this more efficient

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

        legBase.initialRotationOffset = CalculateRotationTarget(leg, legBase);
        legBase.initialOffset = GetOffsetFromOrigin(leg, legBase);

        MoveBaseToGround(leg, legBase);
    }


    public void SnapLegToGround(JointController leg, BaseController legBase)
    {
        legBase.transform.position = leg.movePosition.transform.position;

        MoveBaseToGround(leg, legBase);


        SafeMoveFootToPosition(leg, legBase, legBase.transform.position);

        leg.isStuckToGround = true;

        legBase.groundedPositions = GroundedPositions.UnAligned;

        leg.LockCurrentPosition();
    }


    private float CalculateRotationTarget(JointController leg, BaseController legBase)
    {
        Vector3 forwardDirection = new Vector3(legBase.lastGroundedPosition.transform.position.x - leg.centre.transform.position.x,
            0,
            legBase.lastGroundedPosition.transform.position.z - leg.centre.transform.position.z).normalized;

        Vector3 baseDirection = new Vector3(legBase.transform.position.x - leg.centre.transform.position.x,
            0,
            legBase.transform.position.z - leg.centre.transform.position.z).normalized;

        float offset = Vector3.SignedAngle(forwardDirection, baseDirection, Vector3.up);

        if (offset > 0)
        {
            legBase.rotationTarget = RotationTarget.AntiClockwise;
        }
        else if (offset <= 0)
        {
            legBase.rotationTarget = RotationTarget.Clockwise;
        }

        return offset;
    }


    public bool MoveBaseToGround(JointController leg, BaseController legBase)
    {
        // Actually doing the raycasting
        RaycastHit hit;

        Vector3 rayOrigin = legBase.transform.position;
        rayOrigin.y = legBase.trueGroundedPosition.transform.position.y;

        Debug.DrawLine(rayOrigin + Vector3.up * 3f * _parentTransform.localScale.x, 
            rayOrigin - Vector3.up * 1f * _parentTransform.localScale.x, Color.red ,5);

        if (Physics.Raycast(rayOrigin + Vector3.up * 3f * _parentTransform.localScale.x, -Vector3.up, out hit, 5f * _parentTransform.localScale.x, _groundedMask))
        {

            Vector3 groundedPosition = hit.point;
            groundedPosition.y -= 0.5f * _parentTransform.localScale.x;

            legBase.transform.position = groundedPosition;
        }
        else
        {
            legBase.transform.position = new Vector3(legBase.transform.position.x,
                legBase.trueGroundedPosition.transform.position.y,
                legBase.transform.position.z);
            return false;
        }
        return true;
    }


    private void RotateGroundedPosition(JointController leg, BaseController legBase)
    {
        Vector3 groundedTrueGroundPoint = legBase.trueGroundedPosition.transform.position;
        groundedTrueGroundPoint.y = legBase.transform.position.y;

        Vector3 groundPointPosition = FindGroundPoint(leg, legBase, 0.1f, leg.centre.transform.position, groundedTrueGroundPoint);
        groundPointPosition.y = legBase.trueGroundedPosition.transform.position.y;

        legBase.lastGroundedPosition.transform.position = groundPointPosition;

        leg.defaultRotation = Quaternion.Euler(new Vector3(0, CalculateDefaultRotation(leg, legBase), 0));

        return;
    }


    private static void HandleGroundPointAligned(JointController leg, BaseController legBase)
    {
        legBase.lastGroundedPosition.transform.position = legBase.trueGroundedPosition.transform.position;

        legBase.groundedPositions = GroundedPositions.Aligned;
        leg.defaultRotation = leg.initialRotation;
        legBase.exceededFinalBoundary = false;

        legBase.angleBoundary = 50f;
    }


    private Vector3 FindGroundPoint(JointController leg, BaseController legBase, float step, Vector3 centre, Vector3 end)
    {
        float distanceThroughOrbit = 0f;

        float radius = 1f * _parentTransform.localScale.x; //new Vector2(end.x - centre.x, end.z - centre.z).magnitude;
        float startAngle = Mathf.Atan2(end.z - centre.z, end.x - centre.x) * Mathf.Rad2Deg;
        float rayLength = 1.5f;
        centre.y = legBase.trueGroundedPosition.transform.position.y;
        Vector3 forward = (end - centre).normalized;
        Vector3 clockwisePoint = Vector3.zero, counterClockwisePoint = Vector3.zero;
        Vector3 orbitPosition = Vector3.zero;

        while (distanceThroughOrbit < 20f)
        {
            distanceThroughOrbit += step;

            if(legBase.groundedPositions != GroundedPositions.AntiClockwise)
            {
                // Orbit the end clockwise and cast ray
                // If hits ground return hit.point
                orbitPosition = OrbitPoint(centre, radius, startAngle + distanceThroughOrbit);
                clockwisePoint = TryGetGroundPoint(orbitPosition, rayLength);

                if (clockwisePoint != Vector3.zero)
                {
                    orbitPosition = OrbitPoint(centre, radius, startAngle + distanceThroughOrbit + 25f);
                    clockwisePoint = TryGetGroundPoint(orbitPosition, rayLength);
                    if (clockwisePoint != Vector3.zero)
                    {
                        legBase.groundedPositions = GroundedPositions.Clockwise;
                        legBase.angleBoundary = 60 - distanceThroughOrbit;
                        _arcOffset = GetAngleToLastPosition(leg, legBase);
                        legBase.exceededFinalBoundary = false;

                        return clockwisePoint;
                    }
                }
                clockwisePoint = orbitPosition;
            }

            if (legBase.groundedPositions != GroundedPositions.Clockwise)
            {
                // Orbit the end anticlockwise and cast ray
                // If hits ground return hit.point
                orbitPosition = OrbitPoint(centre, radius, startAngle - distanceThroughOrbit);
                counterClockwisePoint = TryGetGroundPoint(orbitPosition, rayLength);
                if (counterClockwisePoint != Vector3.zero)
                {
                    orbitPosition = OrbitPoint(centre, radius, startAngle - distanceThroughOrbit - 25f);
                    counterClockwisePoint = TryGetGroundPoint(orbitPosition, rayLength);
                    if (counterClockwisePoint != Vector3.zero)
                    {
                        legBase.groundedPositions = GroundedPositions.AntiClockwise;
                        legBase.angleBoundary = 60 - distanceThroughOrbit;
                        _arcOffset = GetAngleToLastPosition(leg, legBase);
                        legBase.exceededFinalBoundary = false;

                        return counterClockwisePoint;
                    }
                }
                counterClockwisePoint = orbitPosition;
            }
        }

        legBase.angleBoundary = 20;

        // Stupid stupid logic here.

        bool leftGrounded = true;
        bool rightGrounded = true;

        if (TryGetGroundPoint(legBase.leftBase.transform.position, 0.5f) == Vector3.zero)
        {
            legBase.rotationTarget = RotationTarget.Clockwise;
            legBase.exceededFinalBoundary = true;
            leftGrounded = false;
        }

        if (TryGetGroundPoint(legBase.rightBase.transform.position, 0.5f) == Vector3.zero)
        {
            legBase.rotationTarget = RotationTarget.AntiClockwise;
            rightGrounded = false;
        }

        if (rightGrounded && leftGrounded)
        {
            if (legBase.leftBase.groundedPositions == GroundedPositions.Aligned && legBase.rightBase.groundedPositions != GroundedPositions.Aligned)
            {
                clockwisePoint = OrbitPoint(centre, radius, startAngle + distanceThroughOrbit + 25f);
                return clockwisePoint;
            }
            if (legBase.rightBase.groundedPositions == GroundedPositions.Aligned && legBase.leftBase.groundedPositions != GroundedPositions.Aligned)
            {
                counterClockwisePoint = OrbitPoint(centre, radius, startAngle - distanceThroughOrbit - 25f);
                return counterClockwisePoint;
            }
        }
        else if (!rightGrounded && leftGrounded)
        {
            clockwisePoint = OrbitPoint(centre, radius, startAngle + distanceThroughOrbit + 25f);
            return clockwisePoint;
        }
        else if (rightGrounded && !leftGrounded)
        {
            counterClockwisePoint = OrbitPoint(centre, radius, startAngle - distanceThroughOrbit - 25f);
            return counterClockwisePoint;
        }
        if (rightGrounded && leftGrounded)
        {
            if(legBase.leftBase.groundedPositions == GroundedPositions.Aligned && legBase.rightBase.groundedPositions != GroundedPositions.Aligned)
            {
                clockwisePoint = OrbitPoint(centre, radius, startAngle + distanceThroughOrbit + 25f);
                return clockwisePoint;
            }
            if (legBase.rightBase.groundedPositions == GroundedPositions.Aligned && legBase.leftBase.groundedPositions != GroundedPositions.Aligned)
            {
                counterClockwisePoint = OrbitPoint(centre, radius, startAngle - distanceThroughOrbit - 25f);
                return counterClockwisePoint;
            }
        }

        Debug.Log($"Failed");

        return legBase.lastGroundedPosition.transform.position;
    }


    private Vector3 OrbitPoint(Vector3 centre, float radius, float angleDeg)
    {
        float angle = angleDeg * Mathf.Deg2Rad;

        return new Vector3(centre.x + Mathf.Cos(angle) * radius, centre.y, centre.z + Mathf.Sin(angle) * radius);
    }


    private Vector3 TryGetGroundPoint(Vector3 pos, float rayLength)
    {

        if (Physics.Raycast(pos + Vector3.up * 1f * _parentTransform.localScale.x,
            Vector3.down, out RaycastHit hit, 
            (rayLength + 1) * _parentTransform.localScale.x, _groundedMask))
        {
            Vector3 hitPoint = hit.point;
            return hit.point;
        }

        Debug.DrawLine(pos + Vector3.up * 1f * _parentTransform.localScale.x,
            pos + Vector3.up * 1f * _parentTransform.localScale.x + Vector3.down * (rayLength + 1) * _parentTransform.localScale.x,
            Color.red, 1);


        return Vector3.zero;
    }


    private float CalculateDefaultRotation(JointController leg, BaseController legBase)
    {
        Vector3 lastGroundedDir = legBase.lastGroundedPosition.transform.position - leg.centre.transform.position;
        lastGroundedDir = Vector3.ProjectOnPlane(lastGroundedDir, Vector3.up).normalized;
        lastGroundedDir = transform.InverseTransformDirection(lastGroundedDir);

        Vector3 initialForward = Vector3.ProjectOnPlane(leg.initialRotation * Vector3.forward, Vector3.up);

        float angleBetweenMarkers = Vector3.SignedAngle(initialForward, lastGroundedDir, Vector3.up);

        float defaultRotation = leg.initialRotation.eulerAngles.y + angleBetweenMarkers;

        return angleBetweenMarkers;
    }


    private void RotateLegToFoot(JointController leg, BaseController legBase)
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

        return;
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

        if (MathF.Abs(offsetAngle) > 1)
        {
            leg.transform.localRotation = Quaternion.Euler(90f, legAngle + offsetAngle, 0f);
        }

        return;
    }


    private bool SafeMoveFootToPosition(JointController leg, BaseController legBase, Vector3 position)
    {
        bool success = leg.MoveFootToPosition(position);

        return !success;
    }


    private void MoveBaseBackForthAndRotate(JointController leg, BaseController legBase, float offsetFromDefault)
    {
        Vector3 position = RotateAlongLeg(leg, legBase);

        position = MoveAlongLeg(leg, legBase, position);

        legBase.transform.position = position;
        RotateLegToBase(leg, legBase);
    }


    private Vector3 RotateAlongLeg(JointController leg, BaseController legBase)
    {
        Vector3 centre = leg.centre.transform.position;
        Vector3 end = legBase.transform.position;

        bool exceededBoundary = false;
        float radius = new Vector2(end.x - centre.x, end.z - centre.z).magnitude;
        float startAngle = Mathf.Atan2(end.z - centre.z, end.x - centre.x) * Mathf.Rad2Deg;
        centre.y = legBase.transform.position.y;
        Vector3 forward = (end - centre).normalized;
        Vector3 orbitPosition = Vector3.zero;

        float angleToLastPosition = GetAngleToLastPosition(leg, legBase);

        if(legBase.groundedPositions != GroundedPositions.Aligned)
        {
            if (angleToLastPosition < -5 && legBase.rotationTarget == RotationTarget.Clockwise)
            {
                legBase.targetRotationOffset = -5f;
                exceededBoundary = true;
            }
            else if (angleToLastPosition > 5f && legBase.rotationTarget == RotationTarget.AntiClockwise)
            {
                legBase.targetRotationOffset = 5;
                exceededBoundary = true;
            }
        }
        else if(angleToLastPosition < -legBase.angleBoundary * 0.8f && legBase.rotationTarget == RotationTarget.Clockwise)
        {
            legBase.targetRotationOffset = -legBase.angleBoundary * 0.8f;
            exceededBoundary = true;
        }
        else if(angleToLastPosition > legBase.angleBoundary * 0.8f && legBase.rotationTarget == RotationTarget.AntiClockwise)
        {
            legBase.targetRotationOffset = legBase.angleBoundary * 0.8f;
            exceededBoundary = true;
        }

        if (!exceededBoundary)
        {

            if (legBase.rotationTarget == RotationTarget.AntiClockwise)
            {
                orbitPosition = OrbitPoint(centre, radius, startAngle + _legRotateSpeed * Time.deltaTime);

                return orbitPosition;
            }

            if (legBase.rotationTarget == RotationTarget.Clockwise)
            {
                orbitPosition = OrbitPoint(centre, radius, startAngle - _legRotateSpeed * Time.deltaTime);

                return orbitPosition;
            }
        }

        return legBase.transform.position;
    }


    private float GetAngleToLastPosition(JointController leg, BaseController legBase)
    {
        // Checks if we are close to target
        Vector3 currentDir = legBase.transform.position - leg.centre.transform.position;
        currentDir.y = 0f;
        currentDir.Normalize();

        Vector3 targetDir = legBase.lastGroundedPosition.transform.position - leg.centre.transform.position;
        targetDir.y = 0f;
        targetDir.Normalize();

        float angle = Vector3.SignedAngle(currentDir, targetDir, Vector3.up);
        return angle;
    }


    private Vector3 MoveAlongLeg(JointController leg, BaseController legBase, Vector3 position)
    {
        Vector3 desiredDirection = (new Vector3(leg.centre.transform.position.x, legBase.transform.position.y, leg.centre.transform.position.z)
            - position).normalized * legBase.direction;

        return position + desiredDirection * _legSpeed * Time.deltaTime;
    }

}
