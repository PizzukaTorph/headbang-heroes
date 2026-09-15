using UnityEngine;

namespace HeadbangHeroes.Gameplay
{
    public sealed class HeadMotionModel : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] float impulse = 190f;
        [SerializeField] float damping = 5.5f;
        [SerializeField] float returnStrength = 9f;
        [SerializeField] float maxAngle = 42f;
        [SerializeField] float maxVelocity = 360f;

        public float Angle { get; private set; }
        public float Velocity { get; private set; }

        public void Bang(float direction, float intensity = 1f)
        {
            Velocity += Mathf.Sign(direction) * impulse * Mathf.Clamp01(intensity);
            Velocity = Mathf.Clamp(Velocity, -maxVelocity, maxVelocity);
        }

        void Update()
        {
            var dt = Time.deltaTime;
            Velocity += -Angle * returnStrength * dt;
            Velocity *= Mathf.Exp(-damping * dt);
            Angle += Velocity * dt;

            if (Mathf.Abs(Angle) > maxAngle)
            {
                Angle = Mathf.Clamp(Angle, -maxAngle, maxAngle);
                Velocity *= -0.15f;
            }

            if (head != null)
                head.localRotation = Quaternion.Euler(0f, 0f, Angle);
        }
    }
}
