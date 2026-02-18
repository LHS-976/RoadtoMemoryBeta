using EnemyControllerScripts;
using UnityEngine;

public class EnemyGroggyState : EnemyBaseState
{
    private float _timer;
    public EnemyGroggyState(EnemyController enemyController, EnemyAnimation enemyAnimation) : base(enemyController, enemyAnimation)
    {
    }

    public override void OnEnter()
    {
        _timer = 0f;
        enemyController.HandleStop();
        enemyController.HandleNavRotationDisable();
        enemyAnimation.PlayGroggy();
    }
    public override void OnUpdate()
    {
        _timer += Time.deltaTime;

        if (_timer >= enemyController.EnemyManager.EnemyStats.groggyDuration)
        {
            enemyController.EnemyManager.RecoverFromGroggy();
            enemyController.ChangeState(enemyController.combatState);
        }
    }
    public override void OnExit()
    {
        enemyAnimation.ClearGroggy();
    }
}
