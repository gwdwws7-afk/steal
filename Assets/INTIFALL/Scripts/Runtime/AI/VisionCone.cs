using UnityEngine;

namespace INTIFALL.AI
{
    public class VisionCone : MonoBehaviour
    {
        [Header("Cone Settings")]
        [SerializeField] private float distance = 15f;
        [SerializeField] private float angle = 60f;
        [SerializeField] private int segments = 32;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(1f, 1f, 0f, 0.2f);
        [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0f, 0.3f);
        [SerializeField] private Color fullAlertColor = new Color(1f, 0f, 0f, 0.4f);

        [Header("References")]
        [SerializeField] private PerceptionModule perception;
        [SerializeField] private EnemyStateMachine stateMachine;

        private Mesh _coneMesh;
        private MeshRenderer _meshRenderer;

        public float Distance => distance;
        public float Angle => angle;

        private void Awake()
        {
            _coneMesh = new Mesh();
            _coneMesh.name = "VisionConeMesh";
            GetComponent<MeshFilter>().mesh = _coneMesh;

            _meshRenderer = GetComponent<MeshRenderer>();
            _meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            _meshRenderer.material.color = normalColor;
        }

        private void Update()
        {
            if (stateMachine == null) return;

            UpdateColor();
            UpdateConeMesh();
        }

        private void UpdateColor()
        {
            Color targetColor = stateMachine.CurrentState switch
            {
                EEnemyState.Unaware => normalColor,
                EEnemyState.Suspicious => alertColor,
                EEnemyState.Searching => alertColor,
                EEnemyState.Alert => fullAlertColor,
                EEnemyState.FullAlert => fullAlertColor,
                _ => normalColor
            };

            _meshRenderer.material.color = Color.Lerp(
                _meshRenderer.material.color,
                targetColor,
                5f * Time.deltaTime
            );
        }

        private void UpdateConeMesh()
        {
            float actualDistance = perception != null ? perception.GetVisionDistance() : distance;
            float actualAngle = perception != null ? perception.GetVisionAngle() : angle;

            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float theta = Mathf.Deg2Rad * (transform.eulerAngles.y - actualAngle * 0.5f + actualAngle * i / segments);
                float x = Mathf.Sin(theta) * actualDistance;
                float z = Mathf.Cos(theta) * actualDistance;
                vertices[i + 1] = new Vector3(x, 0, z);
            }

            for (int i = 0; i < segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            _coneMesh.Clear();
            _coneMesh.vertices = vertices;
            _coneMesh.triangles = triangles;
            _coneMesh.RecalculateNormals();
        }

        public void SetConeParameters(float dist, float ang)
        {
            distance = dist;
            angle = ang;
        }

        private void OnDrawGizmosSelected()
        {
            if (stateMachine == null) return;

            float actualDistance = perception != null ? perception.GetVisionDistance() : distance;
            float actualAngle = perception != null ? perception.GetVisionAngle() : angle;

            Gizmos.color = stateMachine.CurrentState switch
            {
                EEnemyState.Unaware => Color.yellow,
                EEnemyState.Suspicious => Color.magenta,
                EEnemyState.Searching => Color.magenta,
                EEnemyState.Alert => Color.red,
                EEnemyState.FullAlert => Color.red,
                _ => Color.yellow
            };

            Vector3 forward = transform.forward * actualDistance;

            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -actualAngle * 0.5f, 0) * forward);
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, actualAngle * 0.5f, 0) * forward);

            Gizmos.DrawWireSphere(transform.position, actualDistance);
        }
    }
}
