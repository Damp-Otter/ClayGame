using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Scripts.Game.PlayerScripts
{
	public class CharacterData: NetworkBehaviour
    {
        [SerializeField] private float _maxHealth; public float maxHealth { get { return _maxHealth; } set { _maxHealth = value; } }
        [SerializeField] private float _jumpHeight; public float jumpHeight { get { return _jumpHeight; } set { _jumpHeight = value; } }
        [SerializeField] private float _speed; public float speed { get { return _speed; } set { _speed = value; } }

    }
}