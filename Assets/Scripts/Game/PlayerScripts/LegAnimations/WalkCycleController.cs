using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


[Serializable]
public class LegBaseDictionaryItem
{
    [SerializeField] public JointController leg;
    [SerializeField] public GameObject legBase;
}


[Serializable]
public class LegBaseDictionary
{
    [SerializeField] LegBaseDictionaryItem[] thisDictItems;

    public Dictionary<JointController, GameObject> ToDictionary()
    {

        Dictionary<JointController, GameObject> thisDict = new Dictionary<JointController, GameObject>();

        foreach(var item in thisDictItems)
        {
            thisDict.Add(item.leg, item.legBase);
        }

        return thisDict;
    }
}


public class WalkCycleController : MonoBehaviour
{

    private Dictionary<JointController, GameObject> _legsBases;

    [SerializeField] LayerMask _groundedMask;
    [SerializeField] LegBaseDictionary serializedDict;

    public bool isMoving;
    public bool isJumping;


    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();

        int flipped = -1;
        foreach (var (leg, legBase) in _legsBases)
        {
            leg.flipped = flipped;
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
    

    private void ControlLeg(JointController leg, GameObject legBase)
    {
        Vector3 basePosition = legBase.transform.position;

        if (isMoving)
        {
            // Moving base

            Vector3 currentDirection = leg.transform.up * leg.flipped;
            basePosition += currentDirection * 5 *  Time.deltaTime;

            // Moving leg

            Vector3 legPosition = new Vector3(basePosition.x, basePosition.y + 1f, basePosition.z);
            leg.MoveFootToPosition(legPosition);

        }


        legBase.transform.position = basePosition;


        // Lift up leg if needed

        Vector2 offset = new Vector2(legBase.transform.position.x - leg.centre.transform.position.x, legBase.transform.position.z - leg.centre.transform.position.z);

        float distanceFromOrigin = offset.magnitude;

        if (distanceFromOrigin > 1.5f && leg.isStuckToGround)
        {
            SnapLegOffGround(leg, legBase);
        }
        else if (distanceFromOrigin < 0.7f && !leg.isStuckToGround)
        {
            SnapLegToGround(leg, legBase);
        }

        // Reversing foot position

        if (distanceFromOrigin > 1.8f)
        {
            leg.flipped *= -1;
        }
        else if (distanceFromOrigin < 0.4f )
        {
            leg.flipped *= -1;
        }

         
    }


    public void HandleLanding()
    {

        foreach (var (leg, legBase) in _legsBases)
        {
            MoveBaseToGround(legBase);

            SnapLegToGround(leg, legBase);
        }

    }


    public void SnapLegToGround(JointController leg, GameObject legBase)
    {
        Debug.Log("Snapping to ground");

        MoveBaseToGround(legBase);

        leg.MoveFootToPosition(legBase.transform.position);

        leg.isStuckToGround = true;

        leg.LockCurrentPosition();
    }


    public void SnapLegOffGround(JointController leg, GameObject legBase)
    {
        Debug.Log("Snapping off ground");

        Vector3 offset = new Vector3(0, 1f, 0);

        leg.isStuckToGround = false;
        leg.MoveFootByOffest(offset);
    }


    public void MoveBaseToGround(GameObject legbase)
    {
        // Actually doing the raycasting

        RaycastHit hit;

        if (Physics.SphereCast(legbase.transform.position, 0.2f, -Vector3.up, out hit, 2f, _groundedMask))
        {
            legbase.transform.position = hit.point;
        }
    }

}
