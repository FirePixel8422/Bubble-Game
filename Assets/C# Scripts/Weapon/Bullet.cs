using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject owner { get; private set; }
    private float _speed;
    public int damage { get; private set; }
    public void SetVariables(GameObject player, int damage, float speed)
    {
        owner = player;
        this.damage = damage;
        _speed = speed;
    }

    private void Start()
    {
        Destroy(gameObject, 10);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * (Time.deltaTime * _speed));
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner || other.transform.root == owner.transform) return;
        if (other.TryGetComponent(out IDamagable damagable))
        {
            damagable.OnDamaged(damage);
        }
        Destroy(gameObject);
    }
}
