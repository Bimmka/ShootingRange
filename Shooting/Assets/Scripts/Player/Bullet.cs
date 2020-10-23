using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Rigidbody пули")]
    [SerializeField] private Rigidbody rb;
    [Space]

    [Range(1, 200), Header("Скорость полята пули")]
    [SerializeField] private float moveSpeed;

    void Start()
    {
        rb.velocity = transform.forward * moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
