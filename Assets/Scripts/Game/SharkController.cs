using UnityEngine;

namespace FishCollection
{
    public class SharkController : MonoBehaviour
    {
        [SerializeField] private float baseMoveSpeed = 10f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float eatRadius = 1.5f;
        [SerializeField] private float growthRate = 0.1f;
        [SerializeField] private float maxScale = 5f;
        [SerializeField] private float shrinkRate = 0.5f;
        [SerializeField] private float speedReductionPerScale = 0.15f;
        
        private Rigidbody rb;
        private Vector3 movement;
        private int fishEaten = 0;
        private Vector3 initialScale;
        private float currentScaleMultiplier = 1f;
        private float moveSpeed;
        
        public int FishEaten => fishEaten;
        public Vector3 Position => transform.position;
        
        public System.Action<int> OnFishEaten;
        
        void Awake()
        {
            initialScale = transform.localScale;
            moveSpeed = baseMoveSpeed;
            
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
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(transform);
                cube.transform.localPosition = Vector3.zero;
                cube.transform.localScale = Vector3.one * 2f;
            }
            
            gameObject.tag = "Player";
        }
        
        void Update()
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.W)) vertical = 1f;
            if (Input.GetKey(KeyCode.S)) vertical = -1f;
            if (Input.GetKey(KeyCode.A)) horizontal = -1f;
            if (Input.GetKey(KeyCode.D)) horizontal = 1f;
            
            movement = new Vector3(horizontal, 0f, vertical).normalized;
            
            if (movement.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            
            // Shrink over time
            if (currentScaleMultiplier > 1f)
            {
                currentScaleMultiplier -= shrinkRate * Time.deltaTime;
                currentScaleMultiplier = Mathf.Max(1f, currentScaleMultiplier);
                UpdateScale();
            }
        }
        
        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Fish"))
            {
                FishBoid fish = other.GetComponent<FishBoid>();
                if (fish != null && !fish.IsEaten)
                {
                    fish.GetEaten();
                    fishEaten++;
                    GrowShark();
                    OnFishEaten?.Invoke(fishEaten);
                }
            }
        }
        
        void GrowShark()
        {
            currentScaleMultiplier += growthRate;
            currentScaleMultiplier = Mathf.Min(currentScaleMultiplier, maxScale);
            UpdateScale();
        }
        
        void UpdateScale()
        {
            transform.localScale = initialScale * currentScaleMultiplier;
            
            // Update speed based on scale - bigger = slower
            float speedPenalty = (currentScaleMultiplier - 1f) * speedReductionPerScale;
            moveSpeed = baseMoveSpeed * (1f - speedPenalty);
            moveSpeed = Mathf.Max(moveSpeed, baseMoveSpeed * 0.3f); // Minimum 30% of base speed
        }
    }
}