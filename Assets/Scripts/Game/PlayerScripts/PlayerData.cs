using UnityEngine;
using System.Collections;
using Unity.Netcode;
using Assets.Scripts.Game.PlayerScripts;

public class PlayerData : NetworkBehaviour
{
    public CharacterData characterData;
    public SpellData spellData;

    public NetworkVariable<float> Health = new NetworkVariable<float>();
    public NetworkVariable<Vector2> LookInput = new NetworkVariable<Vector2>();

    private bool _isGrounded = false; public bool isGrounded { get { return _isGrounded; } set { _isGrounded = value; } }
    private bool _cooledDown = true; public bool cooledDown { get { return _cooledDown; } set { _cooledDown = value; } }
    private float _senstivityMultiplier = 0.75f; public float senstivityMultiplier { get { return _senstivityMultiplier; } set { _senstivityMultiplier = value; } }
    public NetworkVariable<bool> isAlive = new(true);


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ResetHealthServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ResetHealthServerRpc()
    {
        Health.Value = characterData.maxHealth;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void HealServerRpc(float amount, bool exceedMax)
    {
        Debug.Log($"HEALING {transform.position}");

        if (exceedMax)
        {
            Health.Value = Mathf.Clamp(Health.Value + amount, 0, characterData.maxHealth + 50);
        }
        else
        {
            Health.Value = Mathf.Clamp(Health.Value + amount, 0, characterData.maxHealth);
        }
    }
}
