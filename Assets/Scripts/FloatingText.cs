using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float duration = 0.2f;
    void Start()
    {
        Destroy(gameObject, duration);
    }

}
