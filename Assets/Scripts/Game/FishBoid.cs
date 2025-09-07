using System.Collections.Generic;
using UnityEngine;

namespace FishCollection
{
    public class FishBoid : MonoBehaviour
    {
        [Header("Boid Settings")]
        [SerializeField] public float maxSpeed = 5f;
        [SerializeField] public float maxForce = 10f;
        [SerializeField] private float neighborRadius = 3f;
        [SerializeField] private float avoidRadius = 5f;
        
        [Header("Behavior Weights")]
        [SerializeField] private float separationWeight = 2f;
        [SerializeField] private float alignmentWeight = 1f;
        [SerializeField] private float cohesionWeight = 1f;
        [SerializeField] private float avoidPredatorWeight = 5f;
        [SerializeField] private float boundaryWeight = 3f;
        
        [Header("Health Settings")]
        [SerializeField] private float maxHP = 100f;
        [SerializeField] private float hpDecayRate = 5f;
        
        private Rigidbody rb;
        private Vector3 velocity;
        private Vector3 acceleration;
        private bool isEaten = false;
        private float currentHP;
        private Color baseColor;
        
        private static List<FishBoid> allFish = new List<FishBoid>();
        private Transform predator;
        private Vector3 fieldCenter;
        private float fieldRadius;
        
        public bool IsEaten => isEaten;
        
        void Awake()
        {
            currentHP = maxHP;
            
            // Generate random color in green-cyan-blue range
            GenerateRandomColor();
            
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            }
            
            var renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(transform);
                sphere.transform.localPosition = Vector3.zero;
                sphere.transform.localScale = Vector3.one;
                renderer = sphere.GetComponent<Renderer>();
            }
            
            if (renderer != null)
            {
                renderer.material.color = baseColor;
            }
            
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                gameObject.AddComponent<SphereCollider>().isTrigger = true;
            }
            
            gameObject.tag = "Fish";
            
            velocity = Random.insideUnitSphere * maxSpeed;
            velocity.y = 0;
        }
        
        void GenerateRandomColor()
        {
            // Random hue in green-cyan range (90-180 degrees in HSV)
            float hue = Random.Range(90f, 180f) / 360f;
            // Slight variation in saturation (70-100%)
            float saturation = Random.Range(0.7f, 1f);
            // Slight variation in brightness (70-90%)
            float value = Random.Range(0.7f, 0.9f);
            
            baseColor = Color.HSVToRGB(hue, saturation, value);
        }
        
        void OnEnable()
        {
            allFish.Add(this);
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                predator = player.transform;
            }
        }
        
        void OnDisable()
        {
            allFish.Remove(this);
        }
        
        public void Initialize(Vector3 center, float radius)
        {
            fieldCenter = center;
            fieldRadius = radius;
        }
        
        void Update()
        {
            if (isEaten) return;
            
            // Reduce HP over time
            currentHP -= hpDecayRate * Time.deltaTime;
            if (currentHP <= 0)
            {
                Die();
                return;
            }
            
            // Update color based on HP
            UpdateColorByHP();
            
            acceleration = Vector3.zero;
            
            List<FishBoid> neighbors = GetNeighbors();
            
            if (neighbors.Count > 0)
            {
                Vector3 separation = Separate(neighbors) * separationWeight;
                Vector3 alignment = Align(neighbors) * alignmentWeight;
                Vector3 cohesion = Cohere(neighbors) * cohesionWeight;
                
                acceleration += separation;
                acceleration += alignment;
                acceleration += cohesion;
            }
            
            if (predator != null)
            {
                Vector3 avoidForce = AvoidPredator() * avoidPredatorWeight;
                acceleration += avoidForce;
            }
            
            Vector3 boundaryForce = StayInBounds() * boundaryWeight;
            acceleration += boundaryForce;
            
            velocity += acceleration * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
            
            transform.position += velocity * Time.deltaTime;
            
            if (velocity.magnitude > 0.01f)
            {
                transform.forward = velocity.normalized;
            }
        }
        
        void UpdateColorByHP()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                float hpPercentage = currentHP / maxHP;
                // Blend from base color to red as HP decreases
                Color healthColor;
                if (hpPercentage > 0.5f)
                {
                    // Base color to yellow
                    healthColor = Color.Lerp(Color.yellow, baseColor, (hpPercentage - 0.5f) * 2f);
                }
                else
                {
                    // Yellow to red
                    healthColor = Color.Lerp(Color.red, Color.yellow, hpPercentage * 2f);
                }
                renderer.material.color = healthColor;
            }
        }
        
        void Die()
        {
            isEaten = true;
            allFish.Remove(this);
            Destroy(gameObject);
        }
        
        List<FishBoid> GetNeighbors()
        {
            List<FishBoid> neighbors = new List<FishBoid>();
            
            foreach (FishBoid other in allFish)
            {
                if (other != this && !other.isEaten)
                {
                    float distance = Vector3.Distance(transform.position, other.transform.position);
                    if (distance < neighborRadius)
                    {
                        neighbors.Add(other);
                    }
                }
            }
            
            return neighbors;
        }
        
        Vector3 Separate(List<FishBoid> neighbors)
        {
            Vector3 steer = Vector3.zero;
            int count = 0;
            
            foreach (FishBoid other in neighbors)
            {
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance > 0 && distance < neighborRadius * 0.5f)
                {
                    Vector3 diff = transform.position - other.transform.position;
                    diff.Normalize();
                    diff /= distance;
                    steer += diff;
                    count++;
                }
            }
            
            if (count > 0)
            {
                steer /= count;
                steer.Normalize();
                steer *= maxSpeed;
                steer -= velocity;
                steer = Vector3.ClampMagnitude(steer, maxForce);
            }
            
            return steer;
        }
        
        Vector3 Align(List<FishBoid> neighbors)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            
            foreach (FishBoid other in neighbors)
            {
                sum += other.velocity;
                count++;
            }
            
            if (count > 0)
            {
                sum /= count;
                sum.Normalize();
                sum *= maxSpeed;
                Vector3 steer = sum - velocity;
                steer = Vector3.ClampMagnitude(steer, maxForce);
                return steer;
            }
            
            return Vector3.zero;
        }
        
        Vector3 Cohere(List<FishBoid> neighbors)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            
            foreach (FishBoid other in neighbors)
            {
                sum += other.transform.position;
                count++;
            }
            
            if (count > 0)
            {
                sum /= count;
                return Seek(sum);
            }
            
            return Vector3.zero;
        }
        
        Vector3 AvoidPredator()
        {
            if (predator == null) return Vector3.zero;
            
            float distance = Vector3.Distance(transform.position, predator.position);
            
            if (distance < avoidRadius)
            {
                Vector3 flee = transform.position - predator.position;
                flee.y = 0;
                flee.Normalize();
                flee *= maxSpeed;
                Vector3 steer = flee - velocity;
                steer = Vector3.ClampMagnitude(steer, maxForce * 2f);
                
                float intensity = 1f - (distance / avoidRadius);
                return steer * intensity;
            }
            
            return Vector3.zero;
        }
        
        Vector3 StayInBounds()
        {
            Vector3 steer = Vector3.zero;
            Vector3 toCenter = fieldCenter - transform.position;
            toCenter.y = 0;
            
            float distance = toCenter.magnitude;
            
            if (distance > fieldRadius * 0.8f)
            {
                toCenter.Normalize();
                toCenter *= maxSpeed;
                steer = toCenter - velocity;
                steer = Vector3.ClampMagnitude(steer, maxForce);
                
                float intensity = (distance - fieldRadius * 0.8f) / (fieldRadius * 0.2f);
                steer *= Mathf.Clamp01(intensity);
            }
            
            return steer;
        }
        
        Vector3 Seek(Vector3 target)
        {
            Vector3 desired = target - transform.position;
            desired.y = 0;
            desired.Normalize();
            desired *= maxSpeed;
            
            Vector3 steer = desired - velocity;
            steer = Vector3.ClampMagnitude(steer, maxForce);
            
            return steer;
        }
        
        public void GetEaten()
        {
            if (GetComponent<SpecialFish>() != null)
            {
                Debug.Log("特殊鱼被吃掉了！");
            }
            isEaten = true;
            allFish.Remove(this);
            Destroy(gameObject);
        }
    }
}