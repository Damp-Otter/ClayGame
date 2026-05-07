using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode.Components;
using System;

public class DoorController : NetworkBehaviour
{
    [SerializeField] protected NetworkAnimator _animatorController;
}
