using UnityEngine;

// 일정 시간마다 공중의 랜덤 위치에 공을 만든다
public class BallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;      // 찍어낼 공 프리팹
    [SerializeField] private float spawnInterval = 3f;   // 몇 초마다 만들지
    [SerializeField] private float spawnHeight = 10f;    // 만들 높이
    [SerializeField] private float areaHalfSize = 4.5f;  // 바닥 가운데에서 이 거리 안에서만 만든다

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer -= spawnInterval;
            SpawnBall();
        }
    }

    void SpawnBall()
    {
        float x = Random.Range(-areaHalfSize, areaHalfSize);
        float z = Random.Range(-areaHalfSize, areaHalfSize);
        Instantiate(ballPrefab, new Vector3(x, spawnHeight, z), Quaternion.identity);
    }
}
