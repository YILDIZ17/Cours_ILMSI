using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField]
    private float Speed;

    [SerializeField]
    private float DestroyDistance;

    [SerializeField]
    private int Damages;

    void Update()
    {
        transform.position += new Vector3(0, 0, -Speed * Time.deltaTime);

        if(transform.position.z < DestroyDistance)
        {
            Destroy(gameObject);
        }
    }

    public int Explode()
    {
        Destroy(gameObject);
        return Damages;
    }
}
