using Cinemachine;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    #region Serialise Fields

    [Header("ScriptableObect для игрока с его характеристиками")]
    [SerializeField] private Machine_SO player;

    [Header("Вся турель")]
    [SerializeField] private GameObject turret;

    [Header("Ствол турели")]
    [SerializeField] private GameObject gun;

    [Header("Точка спавна пуль")]
    [SerializeField] private Transform firePoint;

    [Header("Rigidbody корабля")]
    [SerializeField] private Rigidbody rb;

    [Header("FreeLook камера для игры от 3 лица")]
    [SerializeField] private CinemachineFreeLook thirdPlayerView;

    [Header("Virtual камера для игры от 1 лица")]
    [SerializeField] private CinemachineVirtualCamera firstPlayerView;

    [Range(0, 90), Header("Максимальное значение угла поворота турели вдоль оси Y: [0,90]")]
    [SerializeField] private float maxXDegreeTurretRotate = 0f;
    [Space]

    [Range(0, 15), Header("Максимальное значение угла поворота пушки турели вдоль оси Х: [0,15]")]
    [SerializeField] private float maxYDegreeGunRotate = 0f;
    [Space]

    [Range(0, 200), Header("Максимальное значение скорости поворота турели в игре от 1 лица: [0,200]")]
    [SerializeField] private float turretXRotateSpeed = 0f;
    [Space]

    [Range(0, 200), Header("Максимальное значение скорости поворота турели в игре от 3 лица: [0,200]")]
    [SerializeField] private float turretRotateSpeed = 0f;
    [Space]

    [Range(0, 50), Header("Максимальное значение скорости поворота пушки в игре от 1 лица: [0,50]")]
    [SerializeField] private float gunYRotateSpeed = 0f;


    [SerializeField] private LayerMask mask;



    #endregion

    #region Private Fields

    private GameObject bullet;                                  //Prefab пули


    private Vector3 turretRotateOffset = Vector3.zero;         //начальное значение поворота турели

    private bool canShoot = true;


    private CinemachineBasicMultiChannelPerlin virtualCameraNoise;


    #endregion

    #region Unity Events

    public UnityEvent PlayerShoot;                                              //Event при стрельбе персонажа
    public UnityEvent Death;                                                    //Event при смерти персонажа

    #endregion

    public static Action Shooting;                                              //Action, вызываем при стрельбе игрока
    

    private void OnValidate()
    {
        if (maxXDegreeTurretRotate < 0) maxXDegreeTurretRotate = 0;
        if (maxYDegreeGunRotate < 0) maxYDegreeGunRotate = 0;
        if (turretXRotateSpeed < 0) turretXRotateSpeed = 0;
        if (gunYRotateSpeed < 0) gunYRotateSpeed = 0;
    }

    private void Awake()
    {
        firstPlayerView.enabled = false;                                         //отключаем камеру от 1 лица
        turretRotateOffset = turret.transform.localEulerAngles;                //сохраняем начальное значение поворота турели

        bullet = Resources.Load("Bullet", typeof(GameObject)) as GameObject;    //подгружаем Prefab пули

        PlayerSystem.PlayerCantShoot += SetCanShoot;                            //подписываемся на Action, чтобы смотреть может ли игрок стрелять
        PlayerSystem.Death += PlayerDeath;                                      //подписываемя на Action, чтобы смотреть не погиб ли игрок

        UIManager.GameIsPaused += SetCanShoot;                                  // чтобы при паузе игры не стрелять

        virtualCameraNoise = firstPlayerView.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>(); //получаем компонент для добавления тряски камеры
    }

    void Update()
    {
        MovePlayer();                                   
        RotatePlayer();

        ChangeCameraView();

        if (thirdPlayerView.enabled) GunRotateInThirdView();
        else GunRotateInFirstView();

        if (CheckOnOwnShip() && Input.GetMouseButtonDown(0)) Shoot();
    }

    #region Передвижение персонажа
    /// <summary>
    /// Метод для передвижения персонажа вперед/назад
    /// </summary>
    private void MovePlayer()
    {
        float move = Input.GetAxisRaw("Vertical");
        if (move != 0) rb.AddForce(transform.forward * player.Speed * move);
    }

    /// <summary>
    /// Метод для вращение персонажа вправо/влево
    /// </summary>
    private void RotatePlayer()
    {
        float rotate = Input.GetAxisRaw("Horizontal");
        if (rotate != 0) transform.DORotate(transform.eulerAngles + Quaternion.AngleAxis(player.RotateSpeed * Time.deltaTime*rotate, Vector3.up).eulerAngles, 0f);
    }
    #endregion

    #region Работа с камерой от Cinemachine 
    /// <summary>
    /// Метод для переключчения между видам от 3-го или 1-го лица
    /// </summary>
    private void ChangeCameraView()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (thirdPlayerView.enabled)
            {
                ChangeCamera(false, true);
            }
            else
            {
                ChangeCamera(true, false);                
            }
        }
    }

    /// <summary>
    /// Метод для переключения между камерами
    /// </summary>
    /// <param name="thirdView">Флаг для камеры от 3 лица</param>
    /// <param name="firstView">Флаг для камеры от 1 лица</param>
    private void ChangeCamera (bool thirdView, bool firstView)
    {
        firstPlayerView.enabled = firstView;
        thirdPlayerView.enabled = thirdView;
    }
    #endregion

    #region Взаимодействие с турелью при игре от 1 лица
    /// <summary>
    /// Метод для поворота камеры с помощью мышки от 1 лица
    /// </summary>
    private void GunRotateInFirstView()
    {
        float moveX = Input.GetAxis("Mouse X");
        float moveY = Input.GetAxis("Mouse Y");
        if (moveX != 0)
        {
            GunRotateOnAxis(Vector3.up, turretXRotateSpeed, turretRotateOffset.y - maxXDegreeTurretRotate, maxXDegreeTurretRotate + turretRotateOffset.y, Mathf.Sign(moveX), turret, 'y');
        }
        if (moveY != 0)
        {
            GunRotateOnAxis(Vector3.right, gunYRotateSpeed, .5f, maxYDegreeGunRotate, Mathf.Sign(moveY), gun, 'x');
        }
    }

    private void GunRotateInThirdView()
    {        
       turret.transform.rotation = Quaternion.Euler(new Vector3(0, thirdPlayerView.m_XAxis.Value,0) + turretRotateOffset);
    }

    /// <summary>
    /// Метод для поворота объекта (турели/пушки)
    /// </summary>
    /// <param name="vector">Вокруг какой оси поворачиваем</param>
    /// <param name="rotateSpeed">Скорость вращения</param>
    /// <param name="minDegree">Минимальное значение угла для ф-и Lerp</param>
    /// <param name="maxDegree">Максимальное значение угла для ф-и Lerp</param>
    /// <param name="sign">Знак для поворота (+\- 1)</param>
    /// <param name="rotatedObject">Объект, который вращаем</param>
    /// <param name="os">Вращаем x,y,z составляющую</param>
    private void GunRotateOnAxis(Vector3 vector, float rotateSpeed, float minDegree, float maxDegree, float sign, GameObject rotatedObject, char os)
    {
        rotatedObject.transform.Rotate(vector, rotateSpeed * Time.deltaTime * sign);
        var degree = rotatedObject.transform.localEulerAngles;
        switch (os)
        {
            case 'y':
                degree.y = Mathf.Clamp(degree.y, minDegree, maxDegree);
                break;
            case 'x':
                degree.x = Mathf.Clamp(degree.x, minDegree, maxDegree);
                break;
        }
        rotatedObject.transform.localEulerAngles = degree;
    }
    #endregion

    /// <summary>
    /// Метод для выстрела
    /// </summary>
    private void Shoot()
    {

        if (canShoot)
        {
            if (Shooting != null) Shooting();
            PlayerShoot.Invoke();
            if (firstPlayerView.enabled) ShakeCamera(1, 1, 1);
            Instantiate(bullet, firePoint.position, firePoint.rotation);
        }
    }

    /// <summary>
    /// Метод для проверки, что не стреляем по себе
    /// </summary>
    /// <returns></returns>
    private bool CheckOnOwnShip()
    {
        Ray ray = new Ray(firePoint.position, firePoint.transform.forward);
        float distance = 10f;
        if (Physics.Raycast(ray, distance, mask)) return false;
        return true;
    }

    /// <summary>
    /// Метод для тряски камеры
    /// </summary>
    /// <param name="duration">Длительность</param>
    /// <param name="strength">Сила</param>
    /// <param name="frequency">Частота</param>
    private void ShakeCamera(float duration, float strength, float frequency)
    {
        virtualCameraNoise.m_AmplitudeGain = strength;
        virtualCameraNoise.m_FrequencyGain = frequency;
        StartCoroutine(CameraShakeCorutine(duration));
    }

    /// <summary>
    /// Корутина для тряски камеры
    /// </summary>
    /// <param name="shakeTime">Сколько будет трястись камера</param>
    /// <returns></returns>
    IEnumerator CameraShakeCorutine(float shakeTime)
    {
        yield return new WaitForSeconds(shakeTime);
        virtualCameraNoise.m_AmplitudeGain = 0;
        virtualCameraNoise.m_FrequencyGain = 0;
    }

    /// <summary>
    /// Метод для выставления флага готовности к стрельбе
    /// </summary>
    private void SetCanShoot()
    {
        canShoot = !canShoot;
    }

    /// <summary>
    /// Метод смерти персонажа
    /// </summary>
    private void PlayerDeath()
    {
        Death.Invoke();
        rb.velocity = Vector3.zero;
    }

    private void OnDisable()
    {
        PlayerSystem.PlayerCantShoot -= SetCanShoot;
        PlayerSystem.Death -= PlayerDeath;

        UIManager.GameIsPaused -= SetCanShoot;
    }


    

}
