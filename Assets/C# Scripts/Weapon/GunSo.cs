using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Weapons", fileName="Gun")]
public class GunSo : ScriptableObject
{
    public int damage;
    public int reloadTime;
    public int maxAmmo;
    public float shotgunFireRate;
    public Vector3 spread;

    public int minSpeed, maxSpeed;
    public float fireRate;
}
