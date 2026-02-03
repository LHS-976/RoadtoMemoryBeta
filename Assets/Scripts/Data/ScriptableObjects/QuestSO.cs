using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestSO : ScriptableObject
{
    [Header("기본 정보")]
    public int ID;
    public string Title;
    [TextArea] public string Description;

    [Header("목표 및 보상")]
    public string TargetName;
    public int TargetCount;
    public int RewardDataChips;

    [Header("연결")]
    public int NextQuestID;
    public bool IsMainStory;
}
