using System.Collections.Generic;
using UnityEngine;

namespace DogCrush.Presentation
{
    public class ParticleEffectController : MonoBehaviour
    {
        public ParticleSystem particlePrefab;
        public Sprite particleSprite;

        private readonly Queue<ParticleSystem> pool = new Queue<ParticleSystem>();

        public void PlayMatchBurst(Vector3 position, Color color, int count = 12)
        {
            ParticleSystem ps = GetParticleSystem();
            ps.transform.position = position;

            var main = ps.main;
            main.startColor = color;

            var emission = ps.emission;
            emission.SetBurst(0, new ParticleSystem.Burst(0, count));

            ps.Play();
            StartCoroutine(RecycleRoutine(ps, main.duration + main.startLifetime.constantMax));
        }

        private ParticleSystem GetParticleSystem()
        {
            if (pool.Count > 0)
            {
                ParticleSystem ps = pool.Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }
            return CreateNewParticleSystem();
        }

        private ParticleSystem CreateNewParticleSystem()
        {
            GameObject go = new GameObject("PawParticleSystem");
            go.transform.SetParent(transform);

            ParticleSystem ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startLifetime = 0.6f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
            main.gravityModifier = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.3f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = grad;

            ParticleSystemRenderer psr = go.GetComponent<ParticleSystemRenderer>();
            psr.renderMode = ParticleSystemRenderMode.Billboard;
            psr.sortingOrder = 20;

            if (particleSprite != null)
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = particleSprite.texture;
                psr.material = mat;
            }

            return ps;
        }

        private System.Collections.IEnumerator RecycleRoutine(ParticleSystem ps, float delay)
        {
            yield return new WaitForSeconds(delay);
            ps.gameObject.SetActive(false);
            pool.Enqueue(ps);
        }
    }
}
