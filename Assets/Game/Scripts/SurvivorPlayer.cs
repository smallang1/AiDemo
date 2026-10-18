using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorPlayer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer auraRenderer;

        public float CurrentHealth { get; private set; }
        public float MaxHealth { get; private set; }
        public float WeaponDamage { get; private set; }
        public float FireInterval { get; private set; }
        public float MoveSpeed { get; private set; }
        public float PickupRadius { get; private set; }
        public float Regeneration { get; private set; }
        public int ProjectileCount { get; private set; }
        public int Pierce { get; private set; }
        public bool IsAlive => CurrentHealth > 0f;

        private SurvivorGameManager manager;
        private float attackTimer;
        private float invulnerabilityTimer;
        private float hurtFlashTimer;
        private float auraPulse;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down;

        private void Awake()
        {
            manager = SurvivorGameManager.Instance;
            if (manager == null)
            {
                manager = FindObjectOfType<SurvivorGameManager>();
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.sortingOrder = 35;
            }

            ResetForRun();
        }

        private void Update()
        {
            if (manager == null || manager.State != SurvivorGameState.Running || !IsAlive)
            {
                return;
            }

            float delta = Time.deltaTime;
            invulnerabilityTimer = Mathf.Max(0f, invulnerabilityTimer - delta);
            hurtFlashTimer = Mathf.Max(0f, hurtFlashTimer - delta);

            ReadMovement();
            Move(delta);
            Regenerate(delta);
            Animate(delta);
            TryAutoAttack(delta);
        }

        private void ReadMovement()
        {
            moveInput = SurvivorInput.Movement;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveDirection = moveInput.normalized;
            }
        }

        private void Move(float delta)
        {
            Vector3 position = transform.position + (Vector3)(moveInput * (MoveSpeed * delta));
            position.x = Mathf.Clamp(position.x, -SurvivorGameManager.ArenaHalfWidth, SurvivorGameManager.ArenaHalfWidth);
            position.y = Mathf.Clamp(position.y, -SurvivorGameManager.ArenaHalfHeight, SurvivorGameManager.ArenaHalfHeight);
            transform.position = position;
        }

        private void Regenerate(float delta)
        {
            if (Regeneration > 0f && CurrentHealth < MaxHealth)
            {
                CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + Regeneration * delta);
            }
        }

        private void TryAutoAttack(float delta)
        {
            attackTimer -= delta;
            if (attackTimer > 0f)
            {
                return;
            }

            SurvivorEnemy target = SurvivorEnemy.FindNearest(transform.position, 11.8f);
            if (target == null)
            {
                return;
            }

            Vector2 aim = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
            Vector2 origin = (Vector2)transform.position + aim * 0.55f;
            int count = ProjectileCount;
            float totalSpread = count <= 1 ? 0f : 9f * (count - 1);

            for (int i = 0; i < count; i++)
            {
                float angle = count <= 1 ? 0f : -totalSpread * 0.5f + 9f * i;
                Vector2 direction = Quaternion.Euler(0f, 0f, angle) * aim;
                manager.SpawnProjectile(origin, direction, WeaponDamage, 15f, Pierce);
            }

            attackTimer = FireInterval;
            if (auraRenderer != null)
            {
                auraRenderer.transform.localScale = Vector3.one * 1.35f;
            }
        }

        private void Animate(float delta)
        {
            if (visualRoot != null)
            {
                float speed = moveInput.magnitude;
                float bob = Mathf.Sin(Time.time * (7f + speed * 2f)) * (0.025f + speed * 0.02f);
                visualRoot.localPosition = new Vector3(0f, bob, 0f);
                visualRoot.localScale = new Vector3(1f - bob * 0.4f, 1f + bob * 0.4f, 1f);

                if (Mathf.Abs(lastMoveDirection.x) > 0.05f)
                {
                    bodyRenderer.flipX = lastMoveDirection.x < 0f;
                }
            }

            if (bodyRenderer != null)
            {
                Color target = hurtFlashTimer > 0f ? Color.white : Color.white;
                if (hurtFlashTimer > 0f)
                {
                    target = new Color(1f, 0.4f, 0.45f, 1f);
                }

                bodyRenderer.color = Color.Lerp(bodyRenderer.color, target, 18f * delta);
            }

            if (auraRenderer != null)
            {
                auraPulse += delta * 4f;
                float scale = Mathf.Lerp(auraRenderer.transform.localScale.x, 1f + Mathf.Sin(auraPulse) * 0.05f, 10f * delta);
                auraRenderer.transform.localScale = Vector3.one * scale;
                Color auraColor = SurvivorPalette.Teal;
                auraColor.a = 0.11f + Mathf.Sin(auraPulse) * 0.025f;
                auraRenderer.color = auraColor;
            }
        }

        public void TakeDamage(float amount)
        {
            if (invulnerabilityTimer > 0f || !IsAlive || manager == null)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            invulnerabilityTimer = 0.58f;
            hurtFlashTimer = 0.14f;
            manager.FlashDamage();
            manager.ApplyCameraShake(0.14f, 0.22f);
            manager.SpawnBurst(transform.position, SurvivorPalette.Crimson, 7);

            if (CurrentHealth <= 0f)
            {
                manager.PlayerDied();
            }
        }

        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        public void ResetForRun()
        {
            MaxHealth = 135f;
            CurrentHealth = MaxHealth;
            WeaponDamage = 15f;
            FireInterval = 0.48f;
            MoveSpeed = 6.1f;
            PickupRadius = 2.2f;
            Regeneration = 0f;
            ProjectileCount = 1;
            Pierce = 0;
            attackTimer = 0.2f;
            invulnerabilityTimer = 0f;
            hurtFlashTimer = 0f;
            moveInput = Vector2.zero;

            if (bodyRenderer != null)
            {
                bodyRenderer.color = Color.white;
            }
        }

        public void AddDamage(float percent)
        {
            WeaponDamage *= 1f + percent;
        }

        public void AddFireRate(float percent)
        {
            FireInterval = Mathf.Max(0.16f, FireInterval * (1f - percent));
        }

        public void AddMaxHealth(float amount, bool heal)
        {
            MaxHealth += amount;
            if (heal)
            {
                CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            }
        }

        public void AddMoveSpeed(float percent)
        {
            MoveSpeed *= 1f + percent;
        }

        public void AddPierce(int amount)
        {
            Pierce += amount;
        }

        public void AddProjectile()
        {
            ProjectileCount = Mathf.Min(4, ProjectileCount + 1);
        }

        public void AddPickupRadius(float percent)
        {
            PickupRadius *= 1f + percent;
        }

        public void AddRegeneration(float amount)
        {
            Regeneration += amount;
        }
    }
}
