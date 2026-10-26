using UnityEngine;

// 붙인 오브젝트를 Y축 기준으로 계속 회전시킨다
public class Rotator : MonoBehaviour
{
    // private이지만 [SerializeField] 덕분에 Inspector에서 값을 바꿀 수 있다
    [SerializeField] private float rotationSpeed = 90f; // 초당 회전 각도(도)

    void Update()
    {
        // Time.deltaTime(지난 프레임 이후 흐른 시간)을 곱해서
        // 프레임 수와 상관없이 "1초에 rotationSpeed도"만큼 돌게 한다
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
