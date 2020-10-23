using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Точки на левой стороне")]
    [SerializeField] private Transform[] leftPoints;

    [Header("Точки на правойы стороне")]
    [SerializeField] private Transform[] rightPoints;

    [Header("Набор врагов")]
    [SerializeField] private GameObject[] enemies;

    [Header("Смещение по оси Х, для конечных точек врага")]
    [SerializeField] private float xAxisOffset;

    [Header("Интервал между спавнами врагов")]
    [SerializeField] private float spawnInterval;

    [Header("Время, на которое уменьшается время спавна за каждые 10 врагов")]
    [SerializeField] private float changeSpawnInterval;

    [Header("Prefab игрока")]
    [SerializeField] private GameObject player;

    [Header("Место спавна игрока")]
    [SerializeField] private Transform respawn;

    [Header("Место спавна бонусов")]
    [SerializeField] private Transform[] bonusPoints;

    [Header("Смещение для спавна бонусов")]
    [SerializeField] private Vector3 offset;

    [Header("Интервал спавна бонуса")]
    [SerializeField] private float bonusInterval;

    [Header("Бонусы для спавна")]
    [SerializeField] private GameObject[] bonuses;


    public static SpawnManager instance = null;

    public Transform[] LeftWayPoints { get { return leftPoints; } }
    public Transform[] RightWayPoints { get { return rightPoints; } }

    public float AxisOffset { get { return xAxisOffset; } }


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Еще один SpawnManager существует");
            return;
        }
        instance = this;

        Instantiate(player, respawn.position, respawn.rotation);
    }

    private void Start()
    {
        StartCoroutine(CreateEnemy());
        StartCoroutine(CreateBonus());
        PlayerSystem.IncLevelDifficilty += IncDifficilty;


    }

    IEnumerator CreateEnemy()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, leftPoints.Length);
            int randomEnemy = Random.Range(0, enemies.Length);
            Instantiate(enemies[randomEnemy], leftPoints[randomIndex].position, leftPoints[randomIndex].rotation);

            randomIndex = Random.Range(0, rightPoints.Length);
            randomEnemy = Random.Range(0, enemies.Length);
            Instantiate(enemies[randomEnemy], rightPoints[randomIndex].position, rightPoints[randomIndex].rotation);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator CreateBonus()
    {
        while(true)
        {
            yield return new WaitForSeconds(bonusInterval);
            int randomSpawnIndex = Random.Range(0, bonusPoints.Length);
            int randomBonusIndex = Random.Range(0, bonuses.Length);
            Instantiate(bonuses[randomBonusIndex], bonusPoints[randomSpawnIndex].position + new Vector3 (Random.Range(-offset.x, offset.x),0,Random.Range(-offset.z,offset.z)), bonusPoints[randomSpawnIndex].rotation);
        }
    }

    private void IncDifficilty()
    {
        spawnInterval -= changeSpawnInterval;
        if (bonusInterval < 0) bonusInterval = 0;
    }

    private void OnDisable()
    {
        instance = null;
        PlayerSystem.IncLevelDifficilty -= IncDifficilty;
    }
    private void OnDestroy()
    {
        instance = null;
    }

}
