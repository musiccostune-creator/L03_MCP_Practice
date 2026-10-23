using UnityEngine;

// 간단한 AI 선수: 공 뒤로 돌아가서 상대 골대 쪽으로 찬다
[RequireComponent(typeof(Rigidbody))]
public class SoccerPlayer : MonoBehaviour
{
    public int team;                   // 0 = RED, 1 = BLUE
    public SoccerBall ball;
    public Transform opponentGoal;
    public Transform ownGoal;
    public SoccerPlayer opponent;
    public float defendDistance = 3f;  // 수비할 때 공에서 우리 골대 쪽으로 떨어져 서는 거리

    public float moveSpeed = 6f;
    public float kickForce = 11f;
    public float kickRange = 1.1f;
    public float kickCooldown = 0.4f;
    [Range(0f, 1f)] public float accuracy = 0.7f;

    public bool Frozen { get; set; }

    Rigidbody rb;
    float nextKickTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Frozen)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            return;
        }

        Vector3 me = Flat(transform.position);
        Vector3 ballPos = Flat(ball.transform.position);
        Vector3 toGoal = (Flat(opponentGoal.position) - ballPos).normalized;
        Vector3 toBall = ballPos - me;

        // 공과 골대 사이에 서 있으면 옆으로 돌아서 공 뒤로 간다 (자책골 방지)
        Vector3 target;
        float behind = Vector3.Dot(toBall.normalized, toGoal);
        bool opponentCloser = opponent != null &&
            Vector3.Distance(Flat(opponent.transform.position), ballPos) < toBall.magnitude;
        if (opponentCloser && ownGoal != null)
        {
            // 상대가 공에 더 가까우면 공과 우리 골대 사이를 막는다
            target = ballPos + (Flat(ownGoal.position) - ballPos).normalized * defendDistance;
        }
        else if (behind > 0.5f)
        {
            target = ballPos;
        }
        else
        {
            Vector3 side = Vector3.Cross(Vector3.up, toGoal);
            float sign = Vector3.Dot(me - ballPos, side) >= 0f ? 1f : -1f;
            target = ballPos - toGoal * 1.2f + side * sign * (behind < 0f ? 1.5f : 0.5f);
        }

        Vector3 move = target - me;
        Vector3 velocity = move.normalized * moveSpeed * Mathf.Min(1f, move.magnitude);
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        if (velocity.sqrMagnitude > 0.01f)
            rb.MoveRotation(Quaternion.LookRotation(velocity.normalized));

        if (toBall.magnitude < kickRange && behind > 0.2f && Time.time >= nextKickTime)
            Kick(ballPos);
    }

    void Kick(Vector3 ballPos)
    {
        nextKickTime = Time.time + kickCooldown;
        float miss = (1f - accuracy) * 4f;
        Vector3 aim = Flat(opponentGoal.position) + Vector3.forward * Random.Range(-miss, miss);
        Vector3 dir = (aim - ballPos).normalized;

        // 상대가 바로 앞을 막고 있으면 옆으로 비껴 찬다 (서로 밀기만 하는 교착 방지)
        if (opponent != null)
        {
            Vector3 toOpponent = Flat(opponent.transform.position) - ballPos;
            if (toOpponent.magnitude < 2.5f && Vector3.Dot(toOpponent.normalized, dir) > 0.7f)
            {
                // 벽 쪽이 아니라 경기장 가운데 쪽으로 비껴 찬다
                Vector3 left = Quaternion.Euler(0f, -55f, 0f) * dir;
                Vector3 right = Quaternion.Euler(0f, 55f, 0f) * dir;
                dir = Mathf.Abs(ballPos.z + left.z) < Mathf.Abs(ballPos.z + right.z) ? left : right;
            }
        }
        dir.y = 0.12f;
        ball.Body.linearVelocity = dir.normalized * kickForce * Random.Range(0.7f, 1f);
    }

    public void ResetTo(Vector3 position)
    {
        rb.linearVelocity = Vector3.zero;
        rb.position = position;
        transform.position = position;
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}
