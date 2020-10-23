using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu: MonoBehaviour
{
    /// <summary>
    /// Метод для зарузки основного меню
    /// </summary>
    public void LoadMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Метод для загрузки основного уровня
    /// </summary>
    public void LoadLevel()
    {
        SceneManager.LoadScene(1);
        
    }

    /// <summary>
    /// Метод для рестарта уровня
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Метод для выхода из игры
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

}
