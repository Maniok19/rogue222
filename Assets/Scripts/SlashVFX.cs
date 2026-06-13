using UnityEngine;

public class SlashVFX : MonoBehaviour
{
    public float fadeSpeed = 6f; // How fast the slash fades out
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (sr == null) return;

        Color color = sr.color;
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}