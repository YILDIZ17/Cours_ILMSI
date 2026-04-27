using UnityEngine;
using UnityEngine.Rendering;

// Aide simple pour couleurs URP + niveau (couloir off, sol visible).
public static class RunnerVisuals
{
    private static Shader _litShader;

    public static void Paint(Renderer r, Color color)
    {
        if (r == null)
        {
            return;
        }

        Shader s = LitShader();
        if (s == null)
        {
            return;
        }

        Material m = new Material(s);
        if (m.HasProperty("_BaseColor"))
        {
            m.SetColor("_BaseColor", color);
        }
        else
        {
            m.color = color;
        }

        r.sharedMaterial = m;
    }

    public static void PaintChildren(GameObject root, Color color)
    {
        if (root == null)
        {
            return;
        }

        foreach (Renderer r in root.GetComponentsInChildren<Renderer>(true))
        {
            Paint(r, color);
        }
    }

    // Agrandit/réduit pour viser une hauteur en unités (FBX parfois trop petits).
    public static void ResizeHeight(Transform t, float targetHeight)
    {
        if (t == null || targetHeight <= 0f)
        {
            return;
        }

        Renderer[] rs = t.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0)
        {
            return;
        }

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++)
        {
            if (rs[i].enabled)
            {
                b.Encapsulate(rs[i].bounds);
            }
        }

        if (b.size.y < 0.0001f)
        {
            return;
        }

        t.localScale *= targetHeight / b.size.y;
    }

    public static void SetupLevel()
    {
        GameObject level = GameObject.Find("Level");
        if (level != null)
        {
            foreach (MeshRenderer mr in level.GetComponentsInChildren<MeshRenderer>(true))
            {
                mr.enabled = true;
            }

            foreach (Transform child in level.transform)
            {
                if (child.name == "Left" || child.name == "Right" || child.name == "Top")
                {
                    child.gameObject.SetActive(false);
                }
            }

            foreach (Transform child in level.transform)
            {
                MeshRenderer r = child.GetComponent<MeshRenderer>();
                if (r == null)
                {
                    continue;
                }

                Color c = new Color(0.35f, 0.36f, 0.38f);
                if (child.name == "Bot")
                {
                    c = new Color(0.22f, 0.24f, 0.2f);
                }
                else if (child.name == "Top")
                {
                    c = new Color(0.18f, 0.18f, 0.2f);
                }
                else if (child.name == "Left" || child.name == "Right")
                {
                    c = new Color(0.28f, 0.29f, 0.32f);
                }

                Paint(r, c);
            }
        }

        GameObject bot = GameObject.Find("Bot");
        if (bot != null)
        {
            bot.SetActive(true);
            MeshRenderer mr = bot.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = true;
                Paint(mr, new Color(0.5f, 0.52f, 0.55f));
            }
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.16f, 0.17f, 0.2f);

        if (GameObject.Find("RuntimeRunway") == null)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Plane);
            p.name = "RuntimeRunway";
            p.transform.position = new Vector3(0f, 0.02f, 0f);
            p.transform.localScale = new Vector3(18f, 1f, 60f);
            Paint(p.GetComponent<Renderer>(), new Color(0.32f, 0.34f, 0.38f));
            if (level != null)
            {
                p.transform.SetParent(level.transform, true);
            }
        }
    }

    private static Shader LitShader()
    {
        if (_litShader != null)
        {
            return _litShader;
        }

        _litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (_litShader == null)
        {
            _litShader = Shader.Find("Sprites/Default");
        }

        return _litShader;
    }
}
