using UnityEngine;
using PlayerControllerScripts;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] private PlayerController _controller;

    [field: SerializeField] public float CurrentHp { get; private set; }
    [field: SerializeField] public float CurrentStamina { get; private set; }

    public bool IsInvincible { get; private set; }

    private void Awake()
    {
        if (_controller == null) _controller = GetComponent<PlayerController>();
    }
    private void Start()
    {
        Initialize();
    }
    // Update is called once per frame
    void Update()
    {
        HandleStaminaRegenrate();
    }
    private void Initialize()
    {
        if (_controller.playerStats != null)
        {
            CurrentHp = _controller.playerStats.playerMaxHp;
            CurrentStamina = _controller.playerStats.playerMaxStamina;
        }
    }
    private void HandleStaminaRegenrate()
    {
        if (!_controller.IsSprint && CurrentStamina < _controller.playerStats.playerMaxStamina)
        {
            CurrentStamina += _controller.playerStats.staminaRegenrate * Time.deltaTime;

            if (CurrentStamina > _controller.playerStats.playerMaxStamina)
                CurrentStamina = _controller.playerStats.playerMaxStamina;
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

        if(CurrentStamina > _controller.playerStats.playerMaxStamina)
        {
            CurrentStamina = _controller.playerStats.playerMaxStamina;
        }
    }


    public void TakeDamage(float damage)
    {

        /*
        if(IsInvincible)
        {
            return;
        }
        */
        CurrentHp -= damage;
        if (CurrentHp <= 0)
        {
            CurrentHp = 0;
            //Die();
        }
        else
        {
            //_controller.Animator.SetTrigger(PlayerController.AnimIDHit);
            Debug.Log($"피격! 남은 체력: {CurrentHp}");
        }
    }

    public void SetInvincible(bool state)
    {
        IsInvincible = state;
    }
}
