using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Game.Maps.Environments
{
	public class ButtonController: NetworkBehaviour
	{

		public delegate void ButtonPressed(ButtonController doorButton);
		public event ButtonPressed OnButtonPressed;

		public void Activate()
		{
			if (IsServer)
			{
				OnButtonPressed.Invoke(this);
			}
		}

    }
}