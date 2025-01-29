using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HUDUpdater : NetworkBehaviour
{
    public static HUDUpdater Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private TextMeshProUGUI healthTextField;
    [SerializeField] private Slider healthBar;


    [SerializeField] private TextMeshProUGUI killsTextField;
    [SerializeField] private int killCount;
    [SerializeField] private TextMeshProUGUI ammoTextField;




    [ServerRpc(RequireOwnership = false)]
    public void AddKill_ServerRPC(ulong forClientId)
    {
        AddKill_ClientRPC(forClientId);
    }

    [ClientRpc(RequireOwnership = false)]
    private void AddKill_ClientRPC(ulong forClientId)
    {
        if (NetworkManager.LocalClientId != forClientId) return;

        killCount += 1;
        killsTextField.text = killCount.ToString();
    }


    public void UpdateAmmo(int ammoLeft)
    {
        ammoTextField.text = ammoLeft.ToString();
    }

    public void UpdateHealth(int healthLeft)
    {
        healthTextField.text = healthLeft.ToString();

        healthBar.value = healthLeft;
    }
}
