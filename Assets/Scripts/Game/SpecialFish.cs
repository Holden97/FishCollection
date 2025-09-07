using UnityEngine;

namespace FishCollection
{
    [RequireComponent(typeof(FishBoid))]
    public class SpecialFish : MonoBehaviour
    {
        [Header("Special Fish Settings")]
        [SerializeField] private Vector2Int inventoryGridSize = new Vector2Int(2, 3);
        [SerializeField] private bool randomizeGridSize = true;
        [SerializeField] private Vector2Int minGridSize = new Vector2Int(1, 1);
        [SerializeField] private Vector2Int maxGridSize = new Vector2Int(3, 3);
        [SerializeField] private float sizeMultiplier = 2f;
        [SerializeField] private float speedMultiplier = 2f;
        [SerializeField] private float brightnesBoost = 0.3f;
        [SerializeField] private bool glowEffect = true;

        public Vector2Int InventoryGridSize => inventoryGridSize;
        public Color fishBaseColor;
        public int specialFishId;


        void Awake()
        {
            specialFishId = GameManager.FishIdSeed++;
            // Randomize grid size if enabled
            if (randomizeGridSize)
            {
                inventoryGridSize = new Vector2Int(
                    Random.Range(minGridSize.x, maxGridSize.x + 1),
                    Random.Range(minGridSize.y, maxGridSize.y + 1)
                );
            }
            
            // Scale up the fish
            transform.localScale *= sizeMultiplier;
            
            // Increase speed
            FishBoid fishBoid = GetComponent<FishBoid>();
            if (fishBoid != null)
            {
                fishBoid.maxSpeed *= speedMultiplier;
                fishBoid.maxForce *= speedMultiplier;
            }
            
            // Make color brighter
            EnhanceAppearance();
            
            // Add visual indicator
            if (glowEffect)
            {
                AddGlowEffect();
            }
        }
        
        void EnhanceAppearance()
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                fishBaseColor = currentColor;
                
                // Convert to HSV to increase brightness
                Color.RGBToHSV(currentColor, out float h, out float s, out float v);
                
                // Increase value (brightness) and saturation
                v = Mathf.Min(1f, v + brightnesBoost);
                s = Mathf.Min(1f, s + 0.2f);
                
                Color brighterColor = Color.HSVToRGB(h, s, v);
                
                // Add slight emission-like effect by lightening the color
                brighterColor = Color.Lerp(brighterColor, Color.white, 0.2f);
                
                renderer.material.color = brighterColor;
                
                // Make it slightly emissive if using standard shader
                if (renderer.material.HasProperty("_EmissionColor"))
                {
                    renderer.material.EnableKeyword("_EMISSION");
                    renderer.material.SetColor("_EmissionColor", brighterColor * 0.5f);
                }
            }
        }
        
        void AddGlowEffect()
        {
            // Create a larger sphere as glow effect
            GameObject glowSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glowSphere.name = "GlowEffect";
            glowSphere.transform.SetParent(transform);
            glowSphere.transform.localPosition = Vector3.zero;
            glowSphere.transform.localScale = Vector3.one * 1.3f;
            
            // Remove collider from glow sphere
            Destroy(glowSphere.GetComponent<Collider>());
            
            // Set semi-transparent material
            var renderer = glowSphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(1f, 1f, 0.5f, 0.2f);
                
                // Set render queue to transparent
                renderer.material.renderQueue = 3000;
                
                // Enable transparency
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }
    }
}