using UnityEngine;
using PlayerControllerScripts;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerController _playerController;

    [field: SerializeField] public float CurrentHp { get; private set; }
    [field: SerializeField] public float CurrentStamina { get; private set; }

    [SerializeField] private PlayerUIEventChannelSO _playerUIChannel;

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
        HandleStaminaRegenerate();
    }
    private void Initialize()
    {
        if (_playerController.playerStats != null)
        {
            CurrentHp = _playerController.playerStats.playerMaxHp;
            CurrentStamina = _playerController.playerStats.playerMaxStamina;
            isDead = false;

            BroadcastUIUpdate();
        }
    }
    private void HandleStaminaRegenerate()
    {
        if (!_playerController.IsSprint && CurrentStamina < _playerController.playerStats.playerMaxStamina)
        {
            CurrentStamina += _playerController.playerStats.staminaRegenerate * Time.deltaTime;

            if (CurrentStamina > _playerController.playerStats.playerMaxStamina)
                CurrentStamina = _playerController.playerStats.playerMaxStamina;

            BroadcastUIUpdate();
        }
    }
    public bool UseStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            BroadcastUIUpdate();
            return true;
        }
        return false;
    }
    public void ConsumeStamina(float amount)
    {
        CurrentStamina -= amount;
        if (CurrentStamina < 0) CurrentStamina = 0;
        BroadcastUIUpdate();
    }
    public void RestoreStamina(float amount)
    {
        CurrentStamina += amount;

        if(CurrentStamina > _playerController.playerStats.playerMaxStamina)
        {
            CurrentStamina = _playerController.playerStats.playerMaxStamina;
        }
        BroadcastUIUpdate();
    }


    public void TakeDamage(float damage, Vector3 knockBackDir)
    {
        if (isDead) return;
        if (IsInvincible) return; //무적

        CurrentHp -= damage;
        BroadcastUIUpdate();
        if (_playerController != null)
        {
            _playerController.OnHit(knockBackDir);
        }

        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            Die();
        }
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
    #region Broad UI
    private void BroadcastUIUpdate()
    {
        if (_playerUIChannel != null && _playerController.playerStats != null)
        {
            PlayerUIPayload payload = new PlayerUIPayload
            {
                currentHp = CurrentHp,
                maxHp = _playerController.playerStats.playerMaxHp,
                currentStamina = CurrentStamina,
                maxStamina = _playerController.playerStats.playerMaxStamina
            };
            _playerUIChannel.RaiseEvent(payload);
        }
    }
    #endregion
}
