using UnityEngine;

public class PunchDetection : MonoBehaviour
{
    public AnimationAndMovementController PlayerControllerScript;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (PlayerControllerScript.IsAttacking && !triggered)
            {
                //Enemy is Hit
                other.GetComponent<Enemy>().Hit(transform);
                triggered = true;
            }
        }
    }

    private void Update()
    {
        if (!PlayerControllerScript.IsAttacking)
        {
            triggered = false;
        }
    }

}
