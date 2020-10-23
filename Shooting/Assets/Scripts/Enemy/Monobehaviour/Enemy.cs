using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    #region SerialiseFields
    [Header("Характеристики врага")]
    [SerializeField] private Machine_SO enemy;

    [Header("NavMesh Agent врага")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Дистанция")]
    [SerializeField] private float distance = 0f;
    #endregion

    #region Private Fields
    private Transform[] leftPoints;         //точки по левой стороне карты

    private Transform[] rightPoints;        //точки по правой стороне карты

    private bool isLeftSide = false;        //флаг, что враг заспавнился на левой стороне карты

    private float checkInterval = 0.5f;     //значение интервала для проверки на достижение target врага

    private float offset = 0f;              //смещение для точки назначения по оси Х
    #endregion

    public UnityEvent Death;

    private void Awake()
    {
        leftPoints = SpawnManager.instance.LeftWayPoints;               //получаем точки по левой стороне карты
        rightPoints = SpawnManager.instance.RightWayPoints;             //получаем точки по правой стороне карты
        offset = SpawnManager.instance.AxisOffset;                      //получаем смещение для точки достижения по Х
        agent.speed = enemy.Speed;                                      //выставляем скорость противника
        agent.angularSpeed = enemy.RotateSpeed;                         //выставляем скорость поворота противника
    }

    void Start()
    {
        isLeftSide = CheckSpawnSide();
        SetDestination();
        StartCoroutine(CheckDestination());
        
    }
    #region Выставление target  и проверка его достижения

    /// <summary>
    /// Метод для назначения точки, до которой должен дойти враг
    /// </summary>
    private void SetDestination()
    {
        if (agent.enabled)
        {
            if (isLeftSide)                                                                                                                         //если враг был на левой стороне
            {
                agent.destination = rightPoints[Random.Range(0, rightPoints.Length)].position + new Vector3(Random.Range(-offset, offset), 0, 0);      //то выбираем точку из правой части
                isLeftSide = false;
            }
            else
            {
                agent.destination = leftPoints[Random.Range(0, leftPoints.Length)].position + new Vector3(Random.Range(-offset, offset), 0, 0);     //иначе из левой
                isLeftSide = true;
            }
        }
        
    }

    /// <summary>
    /// Метод для определения стороны спавна
    /// </summary>
    /// <returns></returns>
    private bool CheckSpawnSide()
    {
        foreach (var point in leftPoints)                                                       //проверяем с каждой точкей в спавне слевой стороны
        {                                                                           
            if (Vector3.Distance(transform.position, point.position) > distance) return false;  //если расстояние до какой-то из точек больше, чем заданная distance, то это мы заспавнились на правой стороне
        }
        return true;
    }

    /// <summary>
    /// Корутина для определения достижения target противника
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckDestination()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            if (agent.remainingDistance < 2f) SetDestination();
        }
    }

    #endregion    
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bonus Ammo") || !other.CompareTag("Missiles")) EnemyDeath(other);
    }

    /// <summary>
    /// Метод, вызываемый при смерти врага
    /// </summary>
    /// <param name="other">Коллайдер, с которым столкнулся противник</param>
    private void EnemyDeath(Collider other)
    {

        StopAllCoroutines();
        Death.Invoke();
        if (other.CompareTag("Bullet")) PlayerSystem.instance.IncNumberOfKilledEnemies();
        agent.enabled = false;
        Destroy(gameObject, 1.5f);
    }

}
