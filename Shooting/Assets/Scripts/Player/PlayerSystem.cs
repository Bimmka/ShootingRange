using System;
using System.Collections;
using UnityEngine;

public class PlayerSystem : MonoBehaviour
{
    #region Serialize Fields

    [Header("Количество боеприпасов при подборе бонуса")]
    [SerializeField] private int countOfBonusBullet = 0;

    [Header("Время действия бонуса на ракеты")]
    [SerializeField] private float bonusActiveTime = 0f;

    #endregion

    #region Private Fields

    private int numberOfKilledEnemies = 0;

    private int numberOfBullets = 30;

    private float intervalForNextBullet = 2.5f;                 //интервал добавления патрона в инвентарь

    private int incedCountBullet = 1;                           //на какое количество патронов увеличиться боезапас игрока за один intervalForNextBullet

    #endregion

    #region Actions

    public static Action PlayerCantShoot;                       //Action для слежения за тем, что игрок может стрелять

    public static Action Death;                                 //Action для слежения за тем, что игрок погиб

    public static Action<int, int> DisplayBulletAndEnemies;     //Action для отображения в UI количество патронов и количество убитых врагов

    public static Action<int> DisplayResult;                    //Action для отображения в UI при смерти персонажа общее количество убитых врагов

    public static Action IncLevelDifficilty;                    //Action для увеличения сложности игры

    public static PlayerSystem instance = null;

    #endregion

    private void Awake()
    {
        PlayerController.Shooting += DecNumberOfBullet;

        if (instance != null)
        {
            Debug.LogError("Существует еще один PlayerSystem");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        StartCoroutine(IncNumberOfBullet());
        DisplayBulletAndEnemies?.Invoke(numberOfBullets, numberOfKilledEnemies);
    }

    /// <summary>
    /// Корутина для увеличения количества пулей
    /// </summary>
    /// <returns></returns>
    IEnumerator IncNumberOfBullet()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalForNextBullet);
            if (numberOfBullets == 0) if (PlayerCantShoot != null) PlayerCantShoot();

            IncNumberOfBullets(incedCountBullet);
        }
    }

    /// <summary>
    /// Метод для уменьшения количества пуль
    /// </summary>
    private void DecNumberOfBullet()
    {
        numberOfBullets--;
        if (numberOfBullets == 0)  if (PlayerCantShoot != null) PlayerCantShoot();
        DisplayBulletAndEnemies?.Invoke(numberOfBullets, numberOfKilledEnemies);
    }

    /// <summary>
    /// Метод для увеличния количества патронов
    /// </summary>
    /// <param name="count">На какое количество увеличиваем</param>
    private void IncNumberOfBullets(int count)
    {
        numberOfBullets += count;
        DisplayBulletAndEnemies?.Invoke(numberOfBullets,numberOfKilledEnemies);
    }

    /// <summary>
    /// Метод для смерти персонажа
    /// </summary>
    private void PlayerDeath()
    {
        StopAllCoroutines();
        if (DisplayResult!=null) DisplayResult(numberOfKilledEnemies);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bonus Ammo"))
        {
            IncNumberOfBullets(countOfBonusBullet);
            Destroy(other.gameObject);
        }
        if (other.CompareTag("Enemy"))
        {
            if (Death != null) Death();
            PlayerDeath();
        }
    }

    private void OnDisable()
    {
        PlayerController.Shooting -= DecNumberOfBullet;

        instance = null;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// Метод для увеличения количества убитых противников
    /// </summary>
    public void IncNumberOfKilledEnemies()
    {
        numberOfKilledEnemies++;
        DisplayBulletAndEnemies?.Invoke(numberOfBullets, numberOfKilledEnemies);
        if (numberOfKilledEnemies % 5 == 0) IncLevelDifficilty?.Invoke();
    }

    
}
