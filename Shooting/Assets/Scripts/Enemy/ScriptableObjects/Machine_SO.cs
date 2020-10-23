using UnityEngine;

[CreateAssetMenu (fileName = "Machine", menuName = "Create Machine", order = 1)]
public class Machine_SO : ScriptableObject
{
    [Header("Скорость движения"), Range(0,200)]
    [SerializeField] private float moveSpeed = 0f;

    [Header("Скорость поворота"), Range(0, 1000)]
    [SerializeField] private float rotateSpeed = 0f;

    public float Speed => moveSpeed;

    public float RotateSpeed => rotateSpeed;
}
