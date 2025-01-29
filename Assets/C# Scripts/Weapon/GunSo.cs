using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName="Weapons", fileName="Gun")]
public class GunSo : ScriptableObject
{
    public int damage;
    public int reloadTime;
    public int maxAmmo;
    public int speed;
    public float fireRate;
}
