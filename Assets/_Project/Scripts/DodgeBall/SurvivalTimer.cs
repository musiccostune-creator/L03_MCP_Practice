using TMPro;
using UnityEngine;

// 생존시간을 초 단위로 세서 글씨로 보여준다. Game Over가 되면 멈춘다
public class SurvivalTimer : MonoBehaviour
{
    [SerializeField] private PlayerController player; // Game Over 여부를 물어볼 플레이어
    [SerializeField] private TMP_Text timeText;       // 시간을 표시할 글씨 (비워두면 같은 오브젝트에서 찾음)

    private float survivalTime;

    void Awake()
    {
        if (timeText == null) timeText = GetComponent<TMP_Text>();
        if (player == null) player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player != null && player.IsGameOver) return; // Game Over면 시간 정지

        survivalTime += Time.deltaTime;
        timeText.text = "Time: " + Mathf.FloorToInt(survivalTime) + "s";
    }
}
