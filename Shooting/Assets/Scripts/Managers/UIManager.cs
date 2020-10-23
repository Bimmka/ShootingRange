using UnityEngine.UI;
using UnityEngine;
using System;

public class UIManager : MonoBehaviour
{
    #region Serialize Fields

    [Header("Текст, в который будет отображаться количество патронов")]
    [SerializeField] private Text numberOfBulletsText;

    [Header("Текст, в который будет отображаться количество убитых врагов")]
    [SerializeField] private Text numberOfKilledEnemiesText;

    [Header("Текст, в который будет отображаться общее количество убитых врагов в deathMenu")]
    [SerializeField] private Text result;

    [Header("Меню, которое появится после смерти персонажа")]
    [SerializeField] private GameObject deathMenu;

    [Header("Меню, которое будет отображаться после нажатия Escape")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Изображение прицела")]
    [SerializeField] private Image crosshair;

    #endregion

    #region Private Fields

    private bool isPaused = false;

    private bool isDead = false;

    #endregion

    public static Action GameIsPaused;

    private void Start()
    {
        deathMenu.SetActive(false);
        pauseMenu.SetActive(false);

        PlayerSystem.DisplayBulletAndEnemies += DisplayInfo;            //подписываем на обновление количества убитых врагов и патронов

        PlayerSystem.Death += PlayerDeath;                              //подписываем на событие смерти персонажа

        PlayerSystem.DisplayResult += DisplayResult;                    //подписываемся на событие при смерти персонажа (вывод результата)

        crosshair.enabled = false;                                      //убираем перекрестие


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isDead) PauseGame();   
        if (Input.GetMouseButtonDown(1)) crosshair.enabled = !crosshair.enabled;
    }

    /// <summary>
    /// Метод для паузы игры
    /// </summary>
    private void PauseGame()
    {
        if (GameIsPaused != null) GameIsPaused();
        if (isPaused)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
        else
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
    }

    /// <summary>
    /// Метод для отображения информации о патронах и количестве убитых врагов
    /// </summary>
    /// <param name="numberOfBullet">Количество патронов</param>
    /// <param name="numberOfKilledEnemies">Количество убитых врагов</param>
    private void DisplayInfo(int numberOfBullet, int numberOfKilledEnemies)
    {
        numberOfBulletsText.text = numberOfBullet.ToString();
        numberOfKilledEnemiesText.text = numberOfKilledEnemies.ToString();
    }

    /// <summary>
    /// Метод для отображения количества убитых врагов при смерти персонажа
    /// </summary>
    /// <param name="count">Количество убитых врагов</param>
    private void DisplayResult(int count)
    {
        result.text = count.ToString();
    }

    /// <summary>
    /// Смерть персонажа
    /// </summary>
    private void PlayerDeath()
    {
        deathMenu.SetActive(true);
        Time.timeScale = 0f;
        isDead = true;
    }

    private void OnDisable()
    {
        PlayerSystem.DisplayBulletAndEnemies -= DisplayInfo;

        PlayerSystem.Death -= PlayerDeath;

        PlayerSystem.DisplayResult -= DisplayResult;
    }

    /// <summary>
    /// Метод для кнопки Continue
    /// </summary>
    public void ContinueGame()
    {
        if (GameIsPaused != null) GameIsPaused();
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
}
