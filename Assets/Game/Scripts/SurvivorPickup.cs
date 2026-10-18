using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorPickup : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Transform visualRoot;

        private static Sprite healSprite;
        private SurvivorGameManager manager;
        private int value;
        private bool isHeal;
        private float age;
        private float phase;

        public void Initialize(SurvivorGameManager gameManager, int pickupValue, bool heal)
        {
            manager = gameManager;
            value = pickupValue;
            isHeal = heal;
            phase = Random.Range(0f, Mathf.PI * 2f);

            if (bodyRenderer != null)
            {
                if (heal)
                {
                    if (healSprite == null)
                    {
                        healSprite = Resources.Load<Sprite>("Art/pickup_heal");
                    }

                    bodyRenderer.sprite = healSprite;
                    bodyRenderer.color = SurvivorPalette.Crimson;
                }
                else
                {
                    bodyRenderer.color = SurvivorPalette.Cyan;
                }
            }
        }

        private void Update()
        {
            if (manager == null || manager.State != SurvivorGameState.Running || manager.Player == null)
            {
                return;
            }

            float delta = Time.deltaTime;
            age += delta;
            if (age > 90f)
            {
                Destroy(gameObject);
                return;
            }

            Vector2 toPlayer = (Vector2)manager.Player.transform.position - (Vector2)transform.position;
            float distance = toPlayer.magnitude;
            float pickupRadius = isHeal ? manager.Player.PickupRadius * 1.25f : manager.Player.PickupRadius;

            if (distance < pickupRadius)
            {
                float speed = Mathf.Lerp(9f, 2.5f, distance / Mathf.Max(0.01f, pickupRadius));
                transform.position += (Vector3)(toPlayer.normalized * speed * delta);
            }

            if (distance < 0.35f)
            {
                Collect();
                return;
            }

            if (visualRoot != null)
            {
                float bob = Mathf.Sin(Time.time * 6f + phase) * 0.09f;
                visualRoot.localPosition = new Vector3(0f, bob, 0f);
                float pulse = 1f + Mathf.Sin(Time.time * 7f + phase) * 0.06f;
                visualRoot.localScale = Vector3.one * pulse;
                visualRoot.Rotate(0f, 0f, delta * (isHeal ? 24f : 55f));
            }
        }

        private void Collect()
        {
            if (isHeal)
            {
                manager.Player.Heal(value);
                manager.ShowToast("+" + value + " 生命");
            }
            else
            {
                manager.AddXp(value);
            }

            manager.SpawnBurst(transform.position, isHeal ? SurvivorPalette.Crimson : SurvivorPalette.Cyan, 4);
            Destroy(gameObject);
        }
    }
}
