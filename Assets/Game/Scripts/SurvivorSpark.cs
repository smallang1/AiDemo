using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorSpark : MonoBehaviour
    {
        private Vector2 velocity;
        private float lifetime;
        private float maxLifetime;
        private float size;
        private SpriteRenderer spriteRenderer;
        private Color startColor;

        public void Initialize(Vector2 initialVelocity, float duration, float initialSize)
        {
            velocity = initialVelocity;
            lifetime = duration;
            maxLifetime = duration;
            size = initialSize;
            spriteRenderer = GetComponent<SpriteRenderer>();
            startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            transform.localScale = Vector3.one * size;
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            lifetime -= delta;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(velocity * delta);
            velocity *= Mathf.Exp(-5f * delta);
            transform.localScale = Vector3.one * size * Mathf.Clamp01(lifetime / maxLifetime);

            if (spriteRenderer != null)
            {
                Color color = startColor;
                color.a = Mathf.Clamp01(lifetime / maxLifetime);
                spriteRenderer.color = color;
            }
        }
    }
}
