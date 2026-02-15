using EnemyControllerScripts;

public abstract class EnemyBaseState
{
    protected EnemyController enemyController;
    protected EnemyAnimation enemyAnimation;

    public EnemyBaseState(EnemyController enemyController, EnemyAnimation enemyAnimation)
    {
        this.enemyController = enemyController;
        this.enemyAnimation = enemyAnimation;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}
