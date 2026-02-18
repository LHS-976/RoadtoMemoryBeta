using PlayerControllerScripts;
using UnityEngine;
using Core;

public class PlayerExecutionState : PlayerBaseState
{
    private float _timer;
    private bool _executionSuccess;
    private LayerMask _enemyLayer;

    public PlayerExecutionState(PlayerController player, Animator animator) : base(player, animator)
    {
        _enemyLayer = LayerMask.GetMask("Enemy");
    }

    public override void OnEnter()
    {
        _timer = 0f;
        _executionSuccess = false;

        FreezeMovementAnimation();
        player.CombatSystem.ForceStopAttack();

        //처형 대상 탐색 → 실패 시 스태미나 소모 없이 즉시 복귀
        if (!TryExecution())
        {
            player.ChangeState(player.combatState);
            return;
        }

        _executionSuccess = true;
        player.playerManager.UseStamina(player.playerStats.executionStaminaCost);
        animator.CrossFadeInFixedTime(PlayerController.AnimIDExecution, 0.1f);
    }

    public override void OnUpdate()
    {
        _timer += Time.deltaTime;

        float totalDuration = player.playerStats.executionStartupTime + player.playerStats.executionRecoveryTime;

        if (_timer >= totalDuration)
        {
            player.ChangeState(player.combatState);
        }
    }

    public override void OnExit() { }

    private bool TryExecution()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position,player.playerStats.executionRange, _enemyLayer);

        //가장 가까운 처형 가능 대상 탐색
        EnemyManager bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (var col in hitEnemies)
        {
            EnemyManager enemy = col.GetComponent<EnemyManager>();
            if (enemy == null || !enemy.IsExecutionTime) continue;

            Vector3 dirToEnemy = (enemy.transform.position - player.transform.position).normalized;

            //전방 시야각 체크
            if (Vector3.Dot(player.transform.forward, dirToEnemy) <= 0.5f) continue;

            float dist = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestTarget = enemy;
            }
        }

        if (bestTarget == null) return false;

        //처형 대상 방향으로 즉시 회전
        Vector3 toTarget = bestTarget.transform.position - player.transform.position;
        toTarget.y = 0;
        if (toTarget.sqrMagnitude > 0.01f)
        {
            player.HandleRotation(toTarget.normalized, isInstant: true);
        }

        OnSuccessExecution(bestTarget);
        return true;
    }

    private void OnSuccessExecution(EnemyManager enemy)
    {
        enemy.HandleExecutionHit();

        Vector3 hitPoint = (player.transform.position + enemy.transform.position) / 2f + Vector3.up;
        GameEventManager.TriggerExecutionSuccess(hitPoint);

        player.playerManager.RestoreStamina(player.playerStats.executionStaminaRestore);
    }
}