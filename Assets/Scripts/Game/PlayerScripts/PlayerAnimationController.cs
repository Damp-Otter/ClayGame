using UnityEngine;
using Unity.Netcode;
using System;

public class PlayerAnimationController : NetworkBehaviour

{

    [SerializeField] private PlayerData _playerData;

    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _damageBar;
    private float _barSize = 1f;

    public override void OnNetworkSpawn()
    {
        _playerData.Health.OnValueChanged += OnHealthChanged;
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
        UpdateHealthBar();
    }

    public override void OnNetworkDespawn()
    {
        if (_playerData != null)
        {
            _playerData.Health.OnValueChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        UpdateHealthBar();
    }


    private void UpdateHealthBar()
    {
        float healthPercent = _playerData.Health.Value / _playerData.characterData.maxHealth;

        Debug.Log($"Health: {healthPercent * 100}%");

        // Update health bar scale
        Vector3 healthScale = _healthBar.transform.localScale;
        healthScale.x = healthPercent * _barSize;
        _healthBar.transform.localScale = healthScale;

        // Update damage bar scale
        Vector3 damageScale = _damageBar.transform.localScale;
        damageScale.x = (1f - healthPercent) * _barSize;
        _damageBar.transform.localScale = damageScale;

        // Update health bar position
        Vector3 healthPos = _healthBar.transform.localPosition;
        healthPos.x = -(1f - healthPercent) * _barSize * 0.5f;
        _healthBar.transform.localPosition = healthPos;

        // Update damage bar position
        Vector3 damagePos = _damageBar.transform.localPosition;
        damagePos.x = healthPercent * _barSize * 0.5f;
        _damageBar.transform.localPosition = damagePos;
    }



}
