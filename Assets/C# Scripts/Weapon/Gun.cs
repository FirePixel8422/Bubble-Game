using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [SerializeField] protected GameObject bubble;
    [SerializeField] private GunSo so;
    [SerializeField] protected Transform shootPos;
    public int remainingAmmo { get; private set; }
    public int maxAmmo { get; private set; }
    private int _reloadTime;
    private Task _currentTask;
    [SerializeField] private Animator animator;
    [HideInInspector] public float fireRate;
    public float shotgunFireRate;

    private void Start()
    {
        maxAmmo = so.maxAmmo;
        _reloadTime = so.reloadTime;
        remainingAmmo = maxAmmo;
        fireRate = so.fireRate;
        shotgunFireRate = so.shotgunFireRate;
        HUDUpdater.Instance.UpdateAmmo(remainingAmmo);
    }

    public virtual void Shoot()
    {
        if (_currentTask != null) return;
        if (remainingAmmo <= 0)
        {
            Reload(_reloadTime);
            return;
        }
        InstantiateBullet_ServerRPC(NetworkManager.LocalClientId);
        remainingAmmo--;

        HUDUpdater.Instance.UpdateAmmo(remainingAmmo);
    }

    public virtual void ShotgunShoot()
    {
        if (_currentTask != null) return;
        if (remainingAmmo <= 0)
        {
            Reload(_reloadTime);
            return;
        }

        for (int i = 0; i < 2; i++)
        {
            InstantiateBullet_ServerRPC(NetworkManager.LocalClientId);
            remainingAmmo -= 3;
        }
        
        HUDUpdater.Instance.UpdateAmmo(remainingAmmo);
    }


    [ServerRpc(RequireOwnership = false)]
    private void InstantiateBullet_ServerRPC(ulong clientId)
    {
        NetworkObject b = Instantiate(bubble, shootPos.position, shootPos.rotation * Quaternion.Euler(Random.Range(-so.spread.x, so.spread.x), Random.Range(-so.spread.y, so.spread.y), Random.Range(-so.spread.z, so.spread.z))).GetComponent<NetworkObject>();
        b.GetComponent<Bullet>().SetVariables(transform.root.gameObject, so.damage, Random.Range(so.minSpeed, so.maxSpeed));
        b.SpawnWithOwnership(clientId);
    }

    public void PrematureReload()
    {
        if (_currentTask != null) return;
        int t = _reloadTime - (remainingAmmo * maxAmmo * 10);
        Reload(t);
    }
    private async void Reload(int time)
    {
        _currentTask = Task.Delay(_reloadTime);
        await _currentTask;
        remainingAmmo = maxAmmo;

        HUDUpdater.Instance.UpdateAmmo(remainingAmmo);

        _currentTask = null;
    }
}
