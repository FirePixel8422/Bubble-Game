using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class HealingPad : NetworkBehaviour
{
    [SerializeField] private GameObject bubbleGO, crossGO;

    [SerializeField] private float _bobStepHeight = 0.5f;
    [SerializeField] private float _bobSpeed = 2f;

    [SerializeField] private Vector3 crossRotDir;

    private bool shouldBobBubble = true;
    private float _originalY;
    private bool onCooldown = false;

    [SerializeField] int healingFactor;
    [SerializeField] float healCooldownTime;
    private void Awake()
    {
        if (!IsServer) return;

        _originalY = bubbleGO.transform.position.y;
        StartCoroutine(BobObject(bubbleGO, _bobSpeed, _bobStepHeight));
    }


    private IEnumerator BobObject(GameObject objectToBob, float bobSpeed, float bobStepHeight)
    {
        float timePassed = 0f;

        while (shouldBobBubble)
        {
            timePassed += Time.deltaTime * bobSpeed;
            float newY = _originalY + Mathf.Sin(timePassed) * bobStepHeight;

            objectToBob.transform.position = new Vector3(objectToBob.transform.position.x, newY, objectToBob.transform.position.z);
            objectToBob.transform.Rotate(crossRotDir * Time.deltaTime * 50f);

            SyncTransform_ClientRPC(objectToBob.transform.position, objectToBob.transform.rotation);

            yield return null;
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void SyncTransform_ClientRPC(Vector3 pos, Quaternion rot)
    {
        bubbleGO.transform.position = pos;
        bubbleGO.transform.rotation = rot;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!IsOwner || onCooldown) return;

        if (other.gameObject.TryGetComponent(out IHealable healable))
        {
            TryPickupHealthBubble_ServerRpc(other.transform.GetComponent<NetworkObject>().NetworkObjectId, NetworkManager.LocalClientId);
        }
    }
    public IEnumerator HealCooldown()
    {
        yield return new WaitForSeconds(healCooldownTime);

        onCooldown = true;

        EnableBubble_ClientRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryPickupHealthBubble_ServerRpc(ulong networkObjectId, ulong clientId)
    {
        if (onCooldown)
        {
            return;
        }

        onCooldown = true;
        StartCoroutine(HealCooldown());

        PickupHealthBubble_ClientRpc(networkObjectId, clientId);
    }


    [ClientRpc(RequireOwnership = false)]
    private void PickupHealthBubble_ClientRpc(ulong networkObjectId, ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            NetworkObject playerNetwork = NetworkManager.SpawnManager.SpawnedObjects[networkObjectId];

            if (playerNetwork.TryGetComponent(out IHealable healable))
            {
                healable.OnHeal(healingFactor);
            }
        }

        bubbleGO.SetActive(false);
    }


    [ClientRpc(RequireOwnership = false)]
    private void EnableBubble_ClientRPC()
    {
        bubbleGO.SetActive(true);
    }
}