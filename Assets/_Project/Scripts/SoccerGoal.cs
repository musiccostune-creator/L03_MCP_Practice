using UnityEngine;

// 골대 안쪽의 트리거. 공이 들어오면 득점 처리
public class SoccerGoal : MonoBehaviour
{
    public int scoringTeam;            // 이 골대에 넣었을 때 점수를 얻는 팀 (0 = RED, 1 = BLUE)
    public SoccerGameManager manager;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SoccerBall>() != null)
            manager.OnGoal(scoringTeam);
    }
}
