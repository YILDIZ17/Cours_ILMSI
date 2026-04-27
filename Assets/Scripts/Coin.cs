using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField]
    private float Speed = 15f;

    [SerializeField]
    private float DestroyDistance = -15f;

    [SerializeField]
    private int Points = 1;

    [SerializeField]
    private string SheepResourcePath = "Art/Collectibles/Sheep/SheepCollectible";

    private bool _collected;

    private const string RuntimeSheepChildName = "RuntimeSheepCollectible";

    private void Start()
    {
        EnsureSheepVisual();
    }

    private void Update()
    {
        transform.position += new Vector3(0f, 0f, -Speed * Time.deltaTime);

        if (transform.position.z < DestroyDistance)
        {
            Destroy(gameObject);
        }
    }

    public int Collect()
    {
        if (_collected)
        {
            return 0;
        }

        _collected = true;
        Destroy(gameObject);
        return Points;
    }

    private void EnsureSheepVisual()
    {
        if (transform.Find(RuntimeSheepChildName) != null || transform.Find("SheepCollectible") != null)
        {
            return;
        }

        GameObject sheepPrefab = Resources.Load<GameObject>(SheepResourcePath);
        if (sheepPrefab == null)
        {
            BuildFallbackSheepVisual();
            EnsureTriggerCollider();
            return;
        }

        GameObject sheep = Instantiate(sheepPrefab, transform);
        sheep.name = RuntimeSheepChildName;
        sheep.transform.localPosition = Vector3.zero;
        sheep.transform.localRotation = Quaternion.identity;
        sheep.transform.localScale = Vector3.one;
        RunnerVisuals.PaintChildren(sheep, new Color(0.95f, 0.95f, 0.98f));
        RunnerVisuals.ResizeHeight(sheep.transform, 1.25f);
        EnsureTriggerCollider();
    }

    private void BuildFallbackSheepVisual()
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = RuntimeSheepChildName;
        body.transform.SetParent(transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = Vector3.one * 0.95f;
        Collider meshCol = body.GetComponent<Collider>();
        if (meshCol != null)
        {
            Destroy(meshCol);
        }

        RunnerVisuals.Paint(body.GetComponent<Renderer>(), new Color(0.95f, 0.95f, 0.98f));
    }

    private void EnsureTriggerCollider()
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.radius = 0.55f;
            sphere.isTrigger = true;
        }
    }
}
