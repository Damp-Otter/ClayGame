using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Assets.Scripts.Game.PlayerScripts;

public class PlayerData : NetworkBehaviour
{


    public CharacterData characterData;
    public SpellData spellData;


    public NetworkVariable<float> Health = new NetworkVariable<float>();
    private bool _isGrounded = false; public bool isGrounded { get { return _isGrounded; } set { _isGrounded = value; } }
    private bool _cooledDown = true; public bool cooledDown { get { return _cooledDown; } set { _cooledDown = value; } }
    private float _senstivityMultiplier = 0.75f; public float senstivityMultiplier { get { return _senstivityMultiplier; } set { _senstivityMultiplier = value; } }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Health.Value = characterData.maxHealth;

    }

}
