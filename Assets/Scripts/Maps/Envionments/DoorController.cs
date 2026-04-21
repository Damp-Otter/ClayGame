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
        bool anySwitchOn = false;

        foreach (var doorSwitch in _switches)
        {
            if (_activeSwitches[doorSwitch])
            {
                Debug.Log("Opening door");
                anySwitchOn = true;
                _animatorController.SetTrigger("OpenDoor");
            }
        }

        if (anySwitchOn == false)
        {
            Debug.Log("Closing door");
            _animatorController.SetTrigger("CloseDoor");
        }
    }
}
