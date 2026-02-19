using UnityEngine;


[CreateAssetMenu(fileName = "NewSceneData", menuName = "Data/Scene Data")]
public class GameSceneSO : ScriptableObject
{
    [Header("Information")]
    public string sceneName;
    public string displayName;

    [Header("BackGround Audio")]
    public AudioClip backgroundMusic;
}
