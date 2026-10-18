using System.Collections.Generic;
using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorEnemy : MonoBehaviour
    {
        private static readonly List<SurvivorEnemy> ActiveEnemies = new List<SurvivorEnemy>();

        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer shadowRenderer;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform healthRoot;
        [SerializeField] private SpriteRenderer healthFill;

        public EnemyArchetype Archetype { get; private set; }
        public bool IsBoss { get; private set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Radius { get; private set; }
        public int XpValue { get; private set; }
        public bool IsAlive => Health > 0f;

        private SurvivorGameManager manager;
        private SurvivorPlayer player;
        private float speed;
        private float contactDamage;
        private float hitFlashTimer;
        private float wobblePhase;
        private Vector2 knockback;
        private Color baseColor = Color.white;
        private Vector3 baseVisualScale = Vector3.one;
        private Vector3 healthFillBaseScale = Vector3.one;

        private void OnEnable()
        {
            if (!ActiveEnemies.Contains(this))
            {
                ActiveEnemies.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        public void Configure(
            EnemyArchetype archetype,
            float difficulty,
            bool boss,
            Sprite sprite,
            SurvivorGameManager gameManager)
        {
            Archetype = archetype;
            IsBoss = boss;
            manager = gameManager;
            player = gameManager.Player;
            wobblePhase = Random.Range(0f, Mathf.PI * 2f);

            float healthScale = 1f + difficulty * 1.25f;
            float speedScale = 1f + difficulty * 0.18f;
            float damageScale = 1f + difficulty * 0.45f;

            switch (archetype)
            {
                case EnemyArchetype.Ghoul:
                    MaxHealth = 30f * healthScale;
                    speed = 1.52f * speedScale;
                    contactDamage = 8f * damageScale;
                    Radius = 0.43f;
                    XpValue = 3;
                    baseColor = new Color(0.73f, 0.9f, 0.7f, 1f);
                    break;
                case EnemyArchetype.Brute:
                    MaxHealth = 100f * healthScale;
                    speed = 1.05f * speedScale;
                    contactDamage = 14f * damageScale;
                    Radius = 0.67f;
                    XpValue = 8;
                    baseColor = new Color(1f, 0.77f, 0.69f, 1f);
                    break;
                case EnemyArchetype.Reaper:
                    MaxHealth = 720f;
                    speed = 0.95f;
                    contactDamage = 18f;
                    Radius = 1.05f;
                    XpValue = 30;
                    baseColor = new Color(1f, 0.58f, 0.62f, 1f);
                    break;
                default:
                    MaxHealth = 9f * healthScale;
                    speed = 2.1f * speedScale;
                    contactDamage = 4f * damageScale;
                    Radius = 0.32f;
                    XpValue = 1;
                    baseColor = new Color(0.77f, 0.71f, 1f, 1f);
                    break;
            }

            if (boss)
            {
                MaxHealth *= 1.2f;
                XpValue = 50;
            }

            Health = MaxHealth;
            if (bodyRenderer != null)
            {
                bodyRenderer.sprite = sprite;
                bodyRenderer.color = baseColor;
                bodyRenderer.sortingOrder = boss ? 33 : 30;
            }

            if (shadowRenderer != null)
            {
                shadowRenderer.color = new Color(0f, 0f, 0f, boss ? 0.45f : 0.28f);
                shadowRenderer.transform.localScale = Vector3.one * (boss ? 1.45f : Radius * 2.4f);
            }

            if (healthRoot != null)
            {
                healthRoot.localPosition = new Vector3(0f, boss ? 1.55f : Radius + 0.48f, 0f);
                healthRoot.localScale = Vector3.one * (boss ? 1.45f : Mathf.Lerp(0.75f, 1f, Radius));
                healthRoot.gameObject.SetActive(false);
            }

            if (healthFill != null)
            {
                healthFillBaseScale = healthFill.transform.localScale;
            }

            baseVisualScale = boss ? Vector3.one * 1.2f : Vector3.one;
            if (visualRoot != null)
            {
                visualRoot.localScale = baseVisualScale;
            }
        }

        private void Update()
        {
            if (!IsAlive || manager == null || player == null || !player.IsAlive ||
                manager.State != SurvivorGameState.Running)
            {
                return;
            }

            float delta = Time.deltaTime;
            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - delta);
            knockback = Vector2.Lerp(knockback, Vector2.zero, 8f * delta);

            Vector2 toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
            float distance = toPlayer.magnitude;
            Vector2 direction = distance > 0.001f ? toPlayer / distance : Vector2.right;

            Vector2 tangent = new Vector2(-direction.y, direction.x);
            float sway = Mathf.Sin(Time.time * (3.4f + wobblePhase) + wobblePhase) * (IsBoss ? 0.08f : 0.18f);
            Vector2 motion = (direction + tangent * sway).normalized * speed + knockback;
            transform.position += (Vector3)(motion * delta);

            Animate(delta, direction, distance);

            if (distance <= Radius + 0.48f)
            {
                player.TakeDamage(contactDamage);
            }
        }

        private void Animate(float delta, Vector2 direction, float distance)
        {
            if (visualRoot != null)
            {
                float bob = Mathf.Sin(Time.time * (5f + wobblePhase) + wobblePhase) * (IsBoss ? 0.025f : 0.055f);
                float squash = Mathf.Abs(bob) * 0.55f;
                visualRoot.localPosition = new Vector3(0f, bob, 0f);
                visualRoot.localScale = new Vector3(
                    baseVisualScale.x * (1f - squash),
                    baseVisualScale.y * (1f + squash),
                    baseVisualScale.z);
            }

            if (bodyRenderer != null)
            {
                if (Mathf.Abs(direction.x) > 0.05f)
                {
                    bodyRenderer.flipX = direction.x > 0f;
                }

                Color target = hitFlashTimer > 0f ? Color.white : baseColor;
                bodyRenderer.color = Color.Lerp(bodyRenderer.color, target, 25f * delta);
            }

            if (healthRoot != null && healthRoot.gameObject.activeSelf)
            {
                healthRoot.rotation = Quaternion.identity;
                healthFill.transform.localScale = new Vector3(
                    healthFillBaseScale.x * Mathf.Clamp01(Health / MaxHealth),
                    healthFillBaseScale.y,
                    healthFillBaseScale.z);
            }
        }

        public void TakeDamage(float amount, Vector2 hitDirection)
        {
            if (!IsAlive)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - amount);
            hitFlashTimer = 0.09f;
            knockback = hitDirection.normalized * (IsBoss ? 0.8f : 2.4f);
            manager.SpawnBurst(transform.position, new Color(1f, 0.72f, 0.38f, 1f), 3);

            if (healthRoot != null)
            {
                healthRoot.gameObject.SetActive(true);
            }

            if (Health <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            manager.OnEnemyKilled(this, transform.position);
            Destroy(gameObject);
        }

        public static SurvivorEnemy FindNearest(Vector2 origin, float maxDistance)
        {
            SurvivorEnemy nearest = null;
            float bestSqr = maxDistance * maxDistance;

            for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
            {
                SurvivorEnemy enemy = ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    ActiveEnemies.RemoveAt(i);
                    continue;
                }

                float sqr = ((Vector2)enemy.transform.position - origin).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        public static SurvivorEnemy FindNearestOnSegment(Vector2 from, Vector2 to, float extraRadius, ISet<SurvivorEnemy> excluded)
        {
            SurvivorEnemy nearest = null;
            float best = float.MaxValue;
            Vector2 segment = to - from;
            float lengthSqr = Mathf.Max(0.0001f, segment.sqrMagnitude);

            for (int i = ActiveEnemies.Count - 1; i >= 0; i--)
            {
                SurvivorEnemy enemy = ActiveEnemies[i];
                if (enemy == null || !enemy.IsAlive || excluded.Contains(enemy))
                {
                    continue;
                }

                Vector2 center = enemy.transform.position;
                float t = Mathf.Clamp01(Vector2.Dot(center - from, segment) / lengthSqr);
                Vector2 closest = from + segment * t;
                float radius = extraRadius + enemy.Radius;
                float sqr = (center - closest).sqrMagnitude;
                if (sqr <= radius * radius && sqr < best)
                {
                    best = sqr;
                    nearest = enemy;
                }
            }

            return nearest;
        }
    }
}
