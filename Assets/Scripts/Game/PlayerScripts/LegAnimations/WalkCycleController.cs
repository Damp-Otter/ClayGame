using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor.Experimental.GraphView;


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

    private Dictionary<JointController, BaseController> _legsBases;

    [SerializeField] LayerMask _groundedMask;
    [SerializeField] LegBaseDictionary serializedDict;

    public bool isMoving;
    public bool isJumping;
    public bool isTurning;

    private float _baseMaxBoundary = 2.5f;
    private float _baseMinBoundary = 0.75f;
    private float _legMaxBoundary = 2f;
    private float _legMinBoundary = 1.25f;

    private float _legSpeed = 4f * 2;

    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        int flipped = -1;
        foreach (var (leg, legBase) in _legsBases)
        {
            legBase.direction = flipped;
            flipped *= -1;
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

        if (isMoving || isTurning)
        {
            // Moving base

            Vector3 currentDirection = leg.transform.up * legBase.direction;
            basePosition += currentDirection * _legSpeed * Time.deltaTime;

            if (isMoving)
            {
                // Moving leg

                Vector3 legPosition = new Vector3(basePosition.x, basePosition.y + 1f, basePosition.z);
                leg.MoveFootToPosition(legPosition);
            }
        }


        legBase.transform.position = basePosition;


        // Lift up leg if needed

        Vector2 offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x, legBase.transform.position.z - leg.centre.transform.position.z);

        float distanceFromOrigin = offset.magnitude;
        
        CheckIfLegPastBoundary(leg, legBase);

        // Reversing foot position

        CheckIfBaseOnBoundary(leg, legBase, distanceFromOrigin);

    }

    private void CheckIfBaseOnBoundary(JointController leg, BaseController legBase, float distanceFromOrigin)
    {
        if (distanceFromOrigin > _baseMaxBoundary && legBase.direction == -1)
        {
            legBase.direction = 1;
            legBase.SetState(2);
        }
        else if (distanceFromOrigin < _baseMinBoundary && legBase.direction == 1)
        {
            legBase.direction = -1;
            legBase.SetState(1);
        }
    }

    private void CheckIfLegPastBoundary(JointController leg, BaseController legBase)
    {
        Vector2 offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x, legBase.transform.position.z - leg.centre.transform.position.z);

        float distanceFromOrigin = offset.magnitude;

        if (distanceFromOrigin > _legMaxBoundary && leg.isStuckToGround)
        {
            SnapLegOffGround(leg, legBase);
        }
        else if (distanceFromOrigin < _legMinBoundary && !leg.isStuckToGround)
        {
            SnapLegToGround(leg, legBase);
        }

        return;
    }

    public void HandleLanding()
    {

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(legBase);

            SnapLegToGround(leg, legBase);
        }

    }


    public void SnapLegToGround(JointController leg, BaseController legBase)
    {
        MoveBaseToGround(legBase);

        leg.MoveFootToPosition(legBase.transform.position);

        leg.isStuckToGround = true;

        leg.LockCurrentPosition();
    }


    public void SnapLegOffGround(JointController leg, BaseController legBase)
    {
        Vector3 offset = new Vector3(0, 1f, 0);

        leg.isStuckToGround = false;
        leg.MoveFootByOffest(offset);
    }


    public void MoveBaseToGround(BaseController legbase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        Vector3 raycastOrigin = new Vector3(legbase.transform.position.x, legbase.transform.position.y + 0.5f, legbase.transform.position.z);

        if (Physics.SphereCast(legbase.transform.position, 0.2f, -Vector3.up, out hit, 2f, _groundedMask))
        {
            legbase.transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning($"Failed to snap {legbase.transform.name} to the ground");
        }
    }

}
