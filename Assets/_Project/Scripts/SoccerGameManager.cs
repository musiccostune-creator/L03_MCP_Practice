using UnityEngine;

// 점수, 경기 시간, 킥오프, 화면 표시 담당
public class SoccerGameManager : MonoBehaviour
{
    public SoccerBall ball;
    public SoccerPlayer[] players;     // 0 = RED, 1 = BLUE
    public Vector3[] startPositions;
    public float matchDuration = 90f;
    public float goalResetDelay = 1.5f;
    public float stuckResetTime = 5f;  // 공이 이 시간 동안 거의 안 움직이면 다시 킥오프

    static readonly string[] TeamNames = { "RED", "BLUE" };

    readonly int[] score = new int[2];
    float timeLeft;
    bool resetting;
    bool matchOver;
    string message;
    Vector3 lastBallCheckPos;
    float stuckTimer;

    void Start()
    {
        timeLeft = matchDuration;
        Kickoff();
    }

    void Update()
    {
        if (matchOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            EndMatch();
            return;
        }

        if (resetting) return;

        Vector3 ballPos = ball.transform.position;
        if (Vector3.Distance(ballPos, lastBallCheckPos) > 1f)
        {
            lastBallCheckPos = ballPos;
            stuckTimer = 0f;
        }
        else
        {
            stuckTimer += Time.deltaTime;
        }

        if (stuckTimer > stuckResetTime || ballPos.y < -2f)
            Kickoff();
    }

    public void OnGoal(int team)
    {
        if (resetting || matchOver) return;

        score[team]++;
        message = TeamNames[team] + " GOAL!";
        resetting = true;
        SetFrozen(true);
        Invoke(nameof(Kickoff), goalResetDelay);
    }

    void Kickoff()
    {
        ball.ResetTo(new Vector3(0f, 0.5f, 0f));
        for (int i = 0; i < players.Length; i++)
            players[i].ResetTo(startPositions[i]);

        // 매번 똑같이 가운데서 부딪치지 않게 공을 위/아래 랜덤 방향으로 굴려준다
        ball.Body.linearVelocity = new Vector3(Random.Range(-1f, 1f), 0f, Random.value < 0.5f ? -4f : 4f);

        lastBallCheckPos = ball.transform.position;
        stuckTimer = 0f;
        resetting = false;
        message = null;
        SetFrozen(false);
    }

    void EndMatch()
    {
        timeLeft = 0f;
        matchOver = true;
        SetFrozen(true);
        message = score[0] == score[1] ? "DRAW!" : TeamNames[score[0] > score[1] ? 0 : 1] + " WINS!";
        Invoke(nameof(Restart), 5f);
    }

    void Restart()
    {
        score[0] = score[1] = 0;
        timeLeft = matchDuration;
        matchOver = false;
        Kickoff();
    }

    void SetFrozen(bool frozen)
    {
        foreach (var p in players) p.Frozen = frozen;
    }

    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 36, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        int seconds = Mathf.CeilToInt(timeLeft);
        string board = $"RED {score[0]} : {score[1]} BLUE    {seconds / 60}:{seconds % 60:00}";
        GUI.Label(new Rect(0, 10, Screen.width, 50), board, style);

        if (!string.IsNullOrEmpty(message))
        {
            style.fontSize = 64;
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(0, Screen.height / 2f - 50, Screen.width, 100), message, style);
        }
    }
}
