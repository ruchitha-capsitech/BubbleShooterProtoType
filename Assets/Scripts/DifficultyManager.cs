using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;
    public int currentLevel = 1;
    private void Awake()
    {
        Instance = this;
    }
    public float GetDifficulty()
    {
        return Mathf.Clamp01(currentLevel / 10f);
    }
    public int GetAllowedColorCount()
    {
        float d = GetDifficulty();
        return Mathf.RoundToInt(Mathf.Lerp(3, 6, d));
    }
    public int GetRowCount()
    {
        float d = GetDifficulty();
        return Mathf.RoundToInt(Mathf.Lerp(3, 14, d));
    }
}
