using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quests/Quest Database")]
public class QuestDatabase : ScriptableObject
{
    public List<QuestSO> allQuests = new List<QuestSO>();

    private Dictionary<int, QuestSO> _questMap;

    public void Initialize()
    {
        _questMap = new Dictionary<int, QuestSO>();
        foreach (var quest in allQuests)
        {
            if(quest != null && !_questMap.ContainsKey(quest.ID))
            {
                _questMap.Add(quest.ID, quest);
            }
        }
    }
    public QuestSO GetQuest(int id)
    {
        if (_questMap == null) Initialize();
        return _questMap.TryGetValue(id, out QuestSO quest) ? quest : null;
    }

    public QuestSO GetFirstMainQuest()
    {
        foreach(var quest in allQuests)
        {
            if (quest != null && quest.IsMainStory) return quest;
        }
        return null;
    }
}
