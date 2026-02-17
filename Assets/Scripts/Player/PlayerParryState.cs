using PlayerControllerScripts;
using UnityEngine;
using Core;

public class PlayerParryState : PlayerBaseState
{
    private float _timer;
    public bool IsParryActive { get; private set; }
    public PlayerParryState(PlayerController player, Animator animator) : base(player, animator)
    {
    }

    public override void OnEnter()
    {
        _timer = 0f;
        IsParryActive = false;
        player.moveSpeed = 0f;

        animator.CrossFadeInFixedTime(PlayerController.AnimIDParry, 0.1f);
        player.playerManager.UseStamina(player.playerStats.parryStaminaCost);
    }
    public override void OnUpdate()
    {
        _timer += Time.deltaTime;
        float startup = player.playerStats.parryStartupTime;
        float active = player.playerStats.parryActiveTime;
        float totalDuration = startup + active + player.playerStats.parryRecoveryTime;

        if(_timer >= startup && _timer <= (startup + active))
        {
            IsParryActive = true;
        }
        else
        {
            IsParryActive = false;
        }
        if(_timer >= totalDuration)
        {
            player.ChangeState(player.combatState);
        }
    }
    public override void OnExit()
    {
        IsParryActive = false;
    }

    public void OnSuccessParry(EnemyManager enemy)
    {
        if (enemy == null) return;

        enemy.HandleParryHit();

        Vector3 hitPoint = (player.transform.position + enemy.transform.position) / 2f + Vector3.up;
        GameEventManager.TriggerParrySuccess(hitPoint);

        player.playerManager.RestoreStamina(50f);
    }
}
