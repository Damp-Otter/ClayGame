using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.Game.Maps.Environments.ButtonController;

namespace Assets.Scripts.Game.Maps.Environments
{
    public class DoorButton : DoorController
    {
        [SerializeField] private List<ButtonController> _buttons;
        [SerializeField] private float _timeBetweenButtonPressed;

        private Dictionary<ButtonController, float> _activeButtons = new Dictionary<ButtonController, float>();


        private void Start()
        {
            foreach (ButtonController buttonController in _buttons)
            {
                _activeButtons.Add(buttonController, 0);
            }

        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {

                // Only for the ThirdMap scene
                foreach (ButtonController buttonController in _buttons)
                {
                    buttonController.OnButtonPressed += OnButtonPressed;
                }
            }

        }


        private void OnButtonPressed(ButtonController buttonController)
        {

            Debug.Log("Button hit");

            if (_buttons.Contains(buttonController))
            {

                _activeButtons[buttonController] = Time.time;
                bool allButtonsDown = true;

                foreach(var (button, timePressed) in _activeButtons)
                {
                    if (timePressed == 0 || timePressed + _timeBetweenButtonPressed < Time.time)
                    {
                        allButtonsDown = false;
                        break;
                    }
                }

                if(allButtonsDown == true)
                {
                    Debug.Log("Opening door");
                    _animatorController.SetTrigger("OpenDoor");
                }

            }

        }

    }
}