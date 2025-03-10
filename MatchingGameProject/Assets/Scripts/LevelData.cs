using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int width;
    public int height;
    public int targetScore;
    public int maxMoves;
}
