using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


[Serializable]
public class LegBaseDictionaryItem
{
    [SerializeField] public LegController leg;
    [SerializeField] public GameObject legBase;
}


[Serializable]
public class LegBaseDictionary
{
    [SerializeField] LegBaseDictionaryItem[] thisDictItems;

    public Dictionary<LegController, GameObject> ToDictionary()
    {

        Dictionary<LegController, GameObject> thisDict = new Dictionary<LegController, GameObject>();

        foreach(var item in thisDictItems)
        {
            thisDict.Add(item.leg, item.legBase);
        }

        return thisDict;
    }
}


public class WalkCycleController : MonoBehaviour
{

    private Dictionary<LegController, GameObject> _legsBases;

    [SerializeField] LayerMask _groundedMask;
    [SerializeField] LegBaseDictionary serializedDict;


    private void Start()
    {
        _legsBases = serializedDict.ToDictionary();


    }


    void Update()
    {
        foreach (var (leg, legBase) in _legsBases)
        {
            ControlLeg(leg, legBase);
        }
    }
    

    private void ControlLeg(LegController leg, GameObject legBase)
    {
        Vector3 basePosition = legBase.transform.position;
        legBase.transform.position += new Vector3(1f * Time.deltaTime, 0, 1f * Time.deltaTime);

        // Lift up leg if needed

        Vector2 offset = new Vector2(basePosition.x - leg.origin.transform.position.x, basePosition.z - leg.origin.transform.position.z);

        float distanceFromOrigin = offset.magnitude;
        
        if(distanceFromOrigin > 3f)
        {
            Debug.Log("Lifting off");

            SnapLegOffGround(leg, legBase);
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


    public void SnapLegToGround(LegController leg, GameObject legBase)
    {
        Vector3 groundPosition = new Vector3(legBase.transform.position.x, legBase.transform.position.y - 0.2f, legBase.transform.position.z);

        leg.MoveFootToPosition(groundPosition);
        leg.isStuckToGround = true;
    }


    public void SnapLegOffGround(LegController leg, GameObject legBase)
    {
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
