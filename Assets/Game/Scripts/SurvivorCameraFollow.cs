using UnityEngine;

namespace Nightfall
{
    public sealed class SurvivorCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothing = 7f;
        [SerializeField] private float cameraSize = 6.6f;

        private Camera cameraComponent;
        private float shakeTimer;
        private float shakeDuration;
        private float shakeIntensity;
        private Vector3 shakeOffset;

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.orthographic = true;
                cameraComponent.orthographicSize = cameraSize;
            }
        }

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            PlaceImmediately();
        }

        public void PlaceImmediately()
        {
            if (target != null)
            {
                transform.position = ClampToArena(target.position);
            }
        }

        public void Shake(float intensity, float duration)
        {
            if (intensity >= shakeIntensity || shakeTimer <= 0f)
            {
                shakeIntensity = intensity;
                shakeDuration = Mathf.Max(0.01f, duration);
                shakeTimer = shakeDuration;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            Vector3 desired = ClampToArena(target.position);
            float blend = 1f - Mathf.Exp(-smoothing * delta);
            Vector3 basePosition = Vector3.Lerp(transform.position - shakeOffset, desired, blend);

            if (shakeTimer > 0f)
            {
                shakeTimer = Mathf.Max(0f, shakeTimer - delta);
                float falloff = shakeDuration > 0f ? shakeTimer / shakeDuration : 0f;
                Vector2 random = Random.insideUnitCircle * (shakeIntensity * falloff);
                shakeOffset = new Vector3(random.x, random.y, 0f);
            }
            else
            {
                shakeOffset = Vector3.Lerp(shakeOffset, Vector3.zero, 18f * delta);
            }

            transform.position = basePosition + shakeOffset;
        }

        private Vector3 ClampToArena(Vector3 position)
        {
            float halfHeight = cameraComponent != null ? cameraComponent.orthographicSize : cameraSize;
            float halfWidth = halfHeight * (Screen.width / Mathf.Max(1f, (float)Screen.height));
            float maxX = Mathf.Max(0f, SurvivorGameManager.ArenaHalfWidth - halfWidth + 0.8f);
            float maxY = Mathf.Max(0f, SurvivorGameManager.ArenaHalfHeight - halfHeight + 0.8f);
            position.x = Mathf.Clamp(position.x, -maxX, maxX);
            position.y = Mathf.Clamp(position.y, -maxY, maxY);
            position.z = -10f;
            return position;
        }
    }
}
