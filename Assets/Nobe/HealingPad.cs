using System.Collections;
using UnityEngine;

public class HealingPad : MonoBehaviour
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
        _originalY = bubbleGO.transform.position.y;
        StartCoroutine(BobObject(bubbleGO, _bobSpeed, _bobStepHeight));
    }

    private void FixedUpdate()
    {
        RotateObjectAlongAxis(crossGO, crossRotDir);
    }

    private void RotateObjectAlongAxis(GameObject objectToRotate, Vector3 rotDir)
    {
        objectToRotate.transform.Rotate(rotDir * Time.deltaTime * 50f); 
    }

    private IEnumerator BobObject(GameObject objectToBob, float bobSpeed, float bobStepHeight)
    {
        float timePassed = 0f;

        while (shouldBobBubble)
        {
            timePassed += Time.deltaTime * bobSpeed;
            float newY = _originalY + Mathf.Sin(timePassed) * bobStepHeight;
            objectToBob.transform.position = new Vector3(objectToBob.transform.position.x, newY, objectToBob.transform.position.z);
            yield return null;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        print(other.name);
        if (onCooldown) { return; }
        if (other.gameObject.TryGetComponent(out IHealable healable))
        {
            healable.OnHeal(healingFactor);
            StartCoroutine(HealCooldown());
        }
    }
    public IEnumerator HealCooldown()
    {
        bubbleGO.SetActive(false);
        onCooldown = true;
        yield return new WaitForSeconds(healCooldownTime);
        bubbleGO.SetActive(true);
        onCooldown = false;
    }
}
