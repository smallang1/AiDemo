using System.Collections.Generic;
using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorProjectile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private TrailRenderer trail;

        private readonly HashSet<SurvivorEnemy> hitEnemies = new HashSet<SurvivorEnemy>();
        private SurvivorGameManager manager;
        private Vector2 velocity;
        private float damage;
        private float lifetime;
        private int hitsRemaining;

        public void Initialize(
            SurvivorGameManager gameManager,
            Vector2 direction,
            float projectileDamage,
            float speed,
            int pierce)
        {
            manager = gameManager;
            velocity = direction.normalized * speed;
            damage = projectileDamage;
            hitsRemaining = Mathf.Max(1, pierce + 1);
            lifetime = 2.8f;
            transform.up = velocity.normalized;

            if (bodyRenderer != null)
            {
                bodyRenderer.color = SurvivorPalette.Teal;
            }

            if (trail != null)
            {
                Color start = SurvivorPalette.Teal;
                start.a = 0.8f;
                trail.startColor = start;
                trail.endColor = new Color(start.r, start.g, start.b, 0f);
                trail.Clear();
            }
        }

        private void Update()
        {
            if (manager == null || manager.State != SurvivorGameState.Running)
            {
                return;
            }

            float delta = Time.deltaTime;
            lifetime -= delta;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 from = transform.position;
            Vector2 to = from + velocity * delta;
            transform.position = to;

            while (hitsRemaining > 0)
            {
                SurvivorEnemy enemy = SurvivorEnemy.FindNearestOnSegment(from, to, 0.16f, hitEnemies);
                if (enemy == null)
                {
                    break;
                }

                Vector2 hitDirection = ((Vector2)enemy.transform.position - from).normalized;
                hitEnemies.Add(enemy);
                enemy.TakeDamage(damage, hitDirection);
                hitsRemaining--;
            }

            if (hitsRemaining <= 0)
            {
                manager.SpawnBurst(transform.position, SurvivorPalette.Cyan, 3);
                Destroy(gameObject);
            }
        }
    }
}
