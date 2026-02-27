using UnityEngine;
using PlayerControllerScripts;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerController _playerController;

    [field: SerializeField] public float CurrentHp { get; private set; }
    [field: SerializeField] public float CurrentStamina { get; private set; }

    [Header("Broadcast Channel")]
    [SerializeField] private PlayerUIEventChannelSO _playerUIChannel;
    [SerializeField] private VoidEventChannelSO _enableCombatChannel;

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
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            if (data != null)
            {
                CurrentHp = data.GetUpgradedMaxHp(_playerController.playerStats.playerMaxHp);
                CurrentStamina = data.GetUpgradedMaxStamina(_playerController.playerStats.playerMaxStamina);
            }
            else
            {
                CurrentHp = _playerController.playerStats.playerMaxHp;
                CurrentStamina = _playerController.playerStats.playerMaxStamina;
            }
            isDead = false;

            BroadcastUIUpdate();
        }
    }
    #region Upgrade/Stats
    public float MaxHp
    {
        get
        {
            float baseHp = _playerController.playerStats.playerMaxHp;
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            return data != null ? data.GetUpgradedMaxHp(baseHp) : baseHp;
        }
    }
    public float MaxStamina
    {
        get
        {
            float baseStamina = _playerController.playerStats.playerMaxStamina;
            GameData data = Core.GameCore.Instance?.DataManager?.CurrentData;
            return data != null ? data.GetUpgradedMaxStamina(baseStamina) : baseStamina;
        }
    }
    #endregion


    private void HandleStaminaRegenerate()
    {
        if (!_playerController.IsSprint && CurrentStamina < MaxStamina)
        {
            CurrentStamina += _playerController.playerStats.staminaRegenerate * Time.deltaTime;
            if (CurrentStamina > MaxStamina) CurrentStamina = MaxStamina;
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

        if(CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
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
            if(!_playerController.isCombatMode)
            {
                _playerController.ToggleCombatMode();
            }

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
    #region Broadcast UI
    private void BroadcastUIUpdate()
    {
        if (_playerUIChannel != null && _playerController.playerStats != null)
        {
            PlayerUIPayload payload = new PlayerUIPayload
            {
                currentHp = CurrentHp,
                maxHp = MaxHp,
                currentStamina = CurrentStamina,
                maxStamina = MaxStamina
            };
            _playerUIChannel.RaiseEvent(payload);
        }
    }
    public void ApplyUpgrades()
    {
        float newMaxHp = MaxHp;
        float newMaxStamina = MaxStamina;

        //현재 HP가 새 최대값보다 낮으면 최대값으로 회복
        if (CurrentHp < newMaxHp)
            CurrentHp = newMaxHp;

        if (CurrentStamina < newMaxStamina)
            CurrentStamina = newMaxStamina;

        BroadcastUIUpdate();
    }
    #endregion
}
