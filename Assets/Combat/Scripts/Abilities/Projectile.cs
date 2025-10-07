using UnityEngine;
using System;

namespace MiniWoW
{
    public class Projectile : MonoBehaviour
    {
        private Transform target;
        private Vector3 startPos;
        private Vector3 targetPosAtFire;
        private float speed;
        private float travelTime;
        private float elapsed;
        private bool useTravelTime;
        private bool homing;
        private Action OnArrive;

        public void Init(Transform target, float speed, float travelTime, bool useTravelTime, bool homing, Action onArrive)
        {
            this.target = target;
            this.speed = speed;
            this.travelTime = Mathf.Max(0.01f, travelTime);
            this.useTravelTime = useTravelTime;
            this.homing = homing;
            this.OnArrive = onArrive;

            startPos = transform.position;
            targetPosAtFire = target ? target.position : transform.position + transform.forward * 5f;
        }

        private void Update()
        {
            if (useTravelTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / travelTime);
                Vector3 end = homing && target ? target.position : targetPosAtFire;
                transform.position = Vector3.Lerp(startPos, end, t);
                if (t >= 1f) Arrive();
            }
            else
            {
                Vector3 end = homing && target ? target.position : targetPosAtFire;
                Vector3 dir = (end - transform.position);
                float dist = dir.magnitude;
                if (dist < 0.05f) { Arrive(); return; }
                dir.Normalize();
                transform.position += dir * speed * Time.deltaTime;
            }
        }

        private void Arrive()
        {
            OnArrive?.Invoke();
            Destroy(gameObject);
        }
    }
}
