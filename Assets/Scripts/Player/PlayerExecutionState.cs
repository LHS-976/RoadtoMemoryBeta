using PlayerControllerScripts;
using UnityEngine;
using Core;

public class PlayerExecutionState : PlayerBaseState
{
    private float _timer;
    public bool IsExecutionActive { get; private set; }

    private const float _executionRange = 3.0f;
    private LayerMask _enemyLayer;
    public PlayerExecutionState(PlayerController player, Animator animator) : base(player, animator)
    {
        _enemyLayer = LayerMask.GetMask("Enemy");
    }

    public override void OnEnter()
    {
        _timer = 0f;
        IsExecutionActive = false;
        player.moveSpeed = 0f;

        animator.CrossFadeInFixedTime(PlayerController.AnimIDParry, 0.1f);
        CheckForExecution();
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
            IsExecutionActive = true;
        }
        else
        {
            IsExecutionActive = false;
        }
        if(_timer >= totalDuration)
        {
            player.ChangeState(player.combatState);
        }
    }
    public override void OnExit()
    {
        IsExecutionActive = false;
    }

    public void OnSuccessExecution(EnemyManager enemy)
    {
        if (enemy == null) return;

        enemy.HandleExecutionHit();

        Vector3 hitPoint = (player.transform.position + enemy.transform.position) / 2f + Vector3.up;
        GameEventManager.TriggerExecutionSuccess(hitPoint);

        player.playerManager.RestoreStamina(50f);
    }
    private void CheckForExecution()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, _executionRange, _enemyLayer);

        foreach(var collider in hitEnemies)
        {
            EnemyManager enemy = collider.GetComponent<EnemyManager>();
            if (enemy == null) continue;

            if(enemy.IsExecutionTime)
            {
                Vector3 dirToEnemy = (enemy.transform.position - player.transform.position).normalized;
                if(Vector3.Dot(player.transform.forward, dirToEnemy) > 0.5f)
                {
                    OnSuccessExecution(enemy);
                    return;
                }
            }
        }
    }
}
