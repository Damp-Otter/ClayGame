using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode.Components;
using System;

public class DoorController : NetworkBehaviour
{

    [SerializeField] private List<SwitchController> _switches;
    [SerializeField] private NetworkAnimator _animatorController;

    private Dictionary<SwitchController, bool> _activeSwitches= new Dictionary<SwitchController, bool>();


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            foreach(var switchController in _switches)
            {
                switchController.OnSwitchChanged += OnSwitchChanged;
                _activeSwitches.Add(switchController, false);
            }
        }

    }

    private void OnSwitchChanged(SwitchController switchController, bool isActive)
    {
        _activeSwitches[switchController] = isActive;

        if(isActive)
        {
            Debug.Log("Opening door");
            _animatorController.SetTrigger("OpenDoor");
        }
        else {
            Debug.Log("Closing door");
            _animatorController.SetTrigger("CloseDoor");
        }

    }
}
