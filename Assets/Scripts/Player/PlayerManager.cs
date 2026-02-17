using UnityEngine;
using PlayerControllerScripts;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerController _playerController;

    [field: SerializeField] private float _currentHp;
    [field: SerializeField] public float CurrentStamina { get; private set; }

    public bool IsInvincible { get; private set; }
    private bool isDead;
    private void Awake()
    {
        if (_playerController == null) _playerController = GetComponent<PlayerController>();
    }
    private void Start()
    {
        Initialize();
    }
    void Update()
    {
        HandleStaminaRegenrate();
    }
    private void Initialize()
    {
        if (_playerController.playerStats != null)
        {
            _currentHp = _playerController.playerStats.playerMaxHp;
            CurrentStamina = _playerController.playerStats.playerMaxStamina;
            isDead = false;
        }
    }
    private void HandleStaminaRegenrate()
    {
        if (!_playerController.IsSprint && CurrentStamina < _playerController.playerStats.playerMaxStamina)
        {
            CurrentStamina += _playerController.playerStats.staminaRegenrate * Time.deltaTime;

            if (CurrentStamina > _playerController.playerStats.playerMaxStamina)
                CurrentStamina = _playerController.playerStats.playerMaxStamina;
        }
    }
    public bool UseStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            return true;
        }
        return false;
    }
    public void ConsumeStamina(float amount)
    {
        CurrentStamina -= amount;
        if (CurrentStamina < 0) CurrentStamina = 0;
    }
    public void RestoreStamina(float amount)
    {
        CurrentStamina += amount;

        if(CurrentStamina > _playerController.playerStats.playerMaxStamina)
        {
            CurrentStamina = _playerController.playerStats.playerMaxStamina;
        }
    }


    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        if (isDead) return;


        _currentHp -= damage;
        if(_playerController != null)
        {
            _playerController.OnHit(knockBackDir);
        }

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Die();
        }
    }

    public bool TryParry(GameObject attacker)
    {
        if(_playerController.CurrentState is PlayerParryState parryState && parryState.IsParryActive)
        {
            EnemyManager enemy = attacker.GetComponent<EnemyManager>();
            if(enemy != null && enemy.isParryTime)
            {
                parryState.OnSuccessParry(enemy);
                return true;
            }
        }
        return false;
    }

    public void SetInvincible(bool state)
    {
        IsInvincible = state;
    }

    public Transform GetTransform()
    {
        return transform;
    }
    private void Die()
    {
        isDead = true;
        _playerController.HandleDie();
    }
}
