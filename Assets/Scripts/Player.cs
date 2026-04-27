using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float LaneWidth = 2.5f;

    [SerializeField]
    private float LaneChangeSpeed = 10f;

    [SerializeField]
    private Rigidbody Body;

    [SerializeField]
    private int MaxHP = 3;

    [SerializeField]
    private Slider HPSlider;

    [SerializeField]
    private Text ScoreText;

    [SerializeField]
    private float JumpForce = 7f;

    [SerializeField]
    private LayerMask GroundLayers;

    [SerializeField]
    private float GroundCheckDistance = 1.1f;

    [SerializeField]
    private GameObject GameOverScreen;

    [SerializeField]
    private float ScorePerSecond = 2f;

    private int _hp;
    private int _score;
    private int _laneIndex;
    private float _timeScoreLeftover;

    private void Start()
    {
        if (Body == null)
        {
            Body = GetComponent<Rigidbody>();
        }

        if (Body != null)
        {
            Body.constraints = RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationY
                | RigidbodyConstraints.FreezeRotationZ
                | RigidbodyConstraints.FreezePositionZ;
        }

        if (GroundLayers.value == 0)
        {
            GroundLayers = ~0;
        }

        GameObject bot = GameObject.Find("Bot");
        if (bot != null && bot.transform.position.y < -1f)
        {
            Vector3 p = bot.transform.position;
            bot.transform.position = new Vector3(p.x, -0.5f, p.z);
        }

        RunnerVisuals.SetupLevel();

        if (Camera.main != null)
        {
            CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
            if (cf == null)
            {
                cf = Camera.main.gameObject.AddComponent<CameraFollow>();
            }

            cf.target = transform;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;
            Camera.main.backgroundColor = new Color(0.12f, 0.13f, 0.16f, 1f);
        }

        CreerBonhommeSimple();

        _hp = MaxHP;
        _laneIndex = 0;
        CreerTexteScoreSiBesoin();

        if (HPSlider != null)
        {
            HPSlider.minValue = 0f;
            HPSlider.maxValue = MaxHP;
            HPSlider.value = _hp;
        }

        AfficherScore();
    }

    private void FixedUpdate()
    {
        if (Body == null)
        {
            return;
        }

        float targetX = _laneIndex * LaneWidth;
        float nextX = Mathf.MoveTowards(transform.position.x, targetX, LaneChangeSpeed * Time.fixedDeltaTime);
        float vx = (nextX - transform.position.x) / Time.fixedDeltaTime;
        Body.linearVelocity = new Vector3(vx, Body.linearVelocity.y, 0f);
    }

    private void Update()
    {
        if (ScorePerSecond > 0f)
        {
            _timeScoreLeftover += ScorePerSecond * Time.deltaTime;
            if (_timeScoreLeftover >= 1f)
            {
                int add = Mathf.FloorToInt(_timeScoreLeftover);
                _timeScoreLeftover -= add;
                _score += add;
                AfficherScore();
            }
        }

        Keyboard k = Keyboard.current;
        if (k == null)
        {
            return;
        }

        if (k.qKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame)
        {
            _laneIndex = Mathf.Clamp(_laneIndex - 1, -1, 1);
        }

        if (k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame)
        {
            _laneIndex = Mathf.Clamp(_laneIndex + 1, -1, 1);
        }

        if ((k.spaceKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame) && AuSol())
        {
            Body.linearVelocity = new Vector3(Body.linearVelocity.x, JumpForce, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Obstacle o = other.GetComponent<Obstacle>();
        if (o != null)
        {
            _hp -= o.Explode();
            if (HPSlider != null)
            {
                HPSlider.value = Mathf.Max(0, _hp);
            }

            if (_hp <= 0)
            {
                enabled = false;
                if (GameOverScreen != null)
                {
                    GameOverScreen.SetActive(true);
                }
            }

            return;
        }

        Coin c = other.GetComponent<Coin>();
        if (c != null)
        {
            _score += c.Collect();
            AfficherScore();
        }
    }

    private bool AuSol()
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, GroundCheckDistance, GroundLayers);
    }

    private void AfficherScore()
    {
        if (ScoreText != null)
        {
            ScoreText.text = "Score : " + _score;
        }
    }

    private void CreerTexteScoreSiBesoin()
    {
        if (ScoreText != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameObject go = new GameObject("ScoreText");
        go.transform.SetParent(canvas.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(24f, -24f);
        rt.sizeDelta = new Vector2(260f, 40f);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 28;
        t.color = Color.white;
        t.alignment = TextAnchor.UpperLeft;
        ScoreText = t;
    }

    private void CreerBonhommeSimple()
    {
        Transform vieux = transform.Find("SimplePlayer");
        if (vieux != null)
        {
            Destroy(vieux.gameObject);
        }

        GameObject root = new GameObject("SimplePlayer");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = new Vector3(0f, 0.9f, 0f);

        GameObject corps = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        corps.transform.SetParent(root.transform, false);
        corps.transform.localScale = new Vector3(0.7f, 1f, 0.6f);
        RunnerVisuals.Paint(corps.GetComponent<Renderer>(), new Color(0.9f, 0.86f, 0.75f));

        GameObject tete = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tete.transform.SetParent(root.transform, false);
        tete.transform.localPosition = new Vector3(0f, 1.15f, 0f);
        tete.transform.localScale = Vector3.one * 0.42f;
        RunnerVisuals.Paint(tete.GetComponent<Renderer>(), new Color(0.96f, 0.84f, 0.72f));

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.enabled = false;
        }
    }
}
