using UnityEngine;


public enum QuestType
{
    Kill,
    Arrive,
    Event
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestSO : ScriptableObject
{
    [Header("Basic Info")]
    public int ID;
    public string Title;
    [TextArea] public string Description;

    [Header("Goal")]
    public QuestType Type;
    public string TargetID;
    public int TargetCount;
    public string TargetScene;

    [Header("Rewards")]
    public int RewardDataChips;

    [Header("Connection")]
    public int NextQuestID;
    public bool IsMainStory;
}
