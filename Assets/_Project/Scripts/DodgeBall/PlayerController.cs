using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// WASD로 플레이어를 바닥 위에서 움직인다 (새 Input System 사용)
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // 초당 이동 거리(m)

    // 다른 스크립트(생존시간 UI 등)가 읽을 수 있게 공개, 바꾸는 건 이 스크립트만
    public bool IsGameOver { get; private set; }

    private Rigidbody rb;
    private Vector3 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 키 입력은 매 프레임 읽는다
    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector3.zero;
            return;
        }

        // Game Over 상태에서 R을 누르면 씬을 처음부터 다시 불러온다
        if (IsGameOver && keyboard.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        float x = 0f;
        float z = 0f;
        if (keyboard.aKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed) z += 1f;

        // 대각선으로 갈 때 더 빨라지지 않도록 길이를 1로 맞춘다
        moveInput = new Vector3(x, 0f, z).normalized;
    }

    // 물리(Rigidbody) 이동은 FixedUpdate에서 한다
    void FixedUpdate()
    {
        if (IsGameOver) return;

        Vector3 velocity = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    // 무언가와 처음 닿는 순간 호출된다 (떨어지는 공, 스치는 공, 바닥에 멈춘 공 모두)
    void OnCollisionEnter(Collision collision)
    {
        if (IsGameOver) return;                          // 여러 번 출력되지 않게
        if (!collision.gameObject.CompareTag("Ball")) return; // 공이 아니면 무시

        IsGameOver = true;
        Debug.Log("Game Over");

        // 그 자리에 멈추고, 이후 공에 맞아도 밀려나지 않게 한다
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
    }
}
