using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Animator animator;

    private Rigidbody rb;
    [SerializeField] private float knockbackStrength = 2f;
    [SerializeField] private float knockbackDuration = 0.2f;
    public GameObject player;
    public GameObject HitVFXPrefab;
    [SerializeField] private float RotationFactorPerFrame = 15.0f;

    private bool isKnockedBack = false;
    private float knockbackTimer;
    private Vector3 knockbackVelocity;

    private GameObject hitEffect;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    public void Hit(Transform executionSource)
    {
        //When enemy is hit by the player
        hitEffect = Instantiate(HitVFXPrefab, executionSource.position, transform.rotation); //Spawn the Hit VFXs
        animator.SetBool("IsHit", true); //Switch to the Body hit animation
        StartCoroutine(Knockback(executionSource)); //Add knockback
    }

    private IEnumerator Knockback(Transform executionSource)
    {
        yield return new WaitForSeconds(0.1f);
        Vector3 fixedTransform = new(transform.position.x, transform.position.y + 2f, transform.position.z);
        Vector3 dir = (fixedTransform - executionSource.position).normalized;
        knockbackVelocity = dir * knockbackStrength;
        knockbackTimer = knockbackDuration;
        isKnockedBack = true;
        Destroy(hitEffect);
    }

    public void OnHitAnimEnd()
    {
        animator.SetBool("IsHit", false);
    }

    private void FixedUpdate()
    {
        HandleRotation();

        if (isKnockedBack)
        {
            //Move backwards when knocked back
            rb.MovePosition(rb.position + knockbackVelocity * Time.fixedDeltaTime);
            knockbackTimer -= Time.fixedDeltaTime;

            if (knockbackTimer < 0f)
            {
                isKnockedBack=false;
                knockbackVelocity = Vector3.zero;
            }
        }
    }

    private void HandleRotation()
    {
        //Always looks at the player
        Vector3 positionToLookAt = player.transform.position;
        Vector3 direction = positionToLookAt - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Quaternion newRotation = Quaternion.Slerp(rb.rotation, targetRotation, RotationFactorPerFrame * Time.fixedDeltaTime);
            rb.MoveRotation(newRotation);
        }
    }
}
