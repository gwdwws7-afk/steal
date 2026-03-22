using UnityEngine;
using INTIFALL.Player;
using INTIFALL.System;

namespace INTIFALL.Environment
{
    public class HangingPoint : MonoBehaviour
    {
        [Header("Hanging Settings")]
        [SerializeField] private float attachRange = 2f;
        [SerializeField] private float detachRange = 3f;
        [SerializeField] private float enemyKillRange = 2f;

        [Header("Visual")]
        [SerializeField] private Text uiPrompt;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material inactiveMaterial;
        [SerializeField] private MeshRenderer pointMesh;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float lateralSpeed = 2f;

        private bool _isOccupied;
        private bool _playerAttached;
        private GameObject _player;
        private PlayerController _playerController;
        private LayerMask _enemyLayer;

        public bool IsOccupied => _isOccupied;
        public bool PlayerAttached => _playerAttached;
        public Vector3 Position => transform.position;

        private void Start()
        {
            _enemyLayer = LayerMask.GetMask("Enemy");

            if (pointMesh != null && inactiveMaterial != null)
                pointMesh.material = inactiveMaterial;
        }

        private void Update()
        {
            if (_playerAttached)
            {
                ShowPrompt("按 E 脱离 / Q 绳技击杀");
                UpdatePlayerHanging();

                if (Vector3.Distance(transform.position, _player.transform.position) > detachRange)
                {
                    Detach();
                }

                CheckForEnemyBelow();
            }
            else
            {
                CheckPlayerProximity();
            }
        }

        private void CheckPlayerProximity()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if (_player == null)
            {
                ShowPrompt("");
                return;
            }

            float distance = Vector3.Distance(transform.position, _player.transform.position);

            if (distance <= attachRange && !_isOccupied)
            {
                ShowPrompt("按 E 抓住悬挂点");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    Attach();
                }
            }
            else
            {
                ShowPrompt("");
            }
        }

        private void UpdatePlayerHanging()
        {
            if (_player == null || _playerController == null) return;

            float lateral = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 lateralMove = transform.right * lateral * lateralSpeed * Time.deltaTime;
            Vector3 ropeMove = Vector3.up * vertical * moveSpeed * Time.deltaTime;

            Vector3 newPos = _player.transform.position + lateralMove + ropeMove;

            if (Vector3.Distance(transform.position, newPos) <= detachRange)
            {
                _player.transform.position = newPos;
            }
        }

        private void Attach()
        {
            if (_player == null) return;

            _playerAttached = true;
            _isOccupied = true;
            _playerController = _player.GetComponent<PlayerController>();

            if (_playerController != null)
                _playerController.AttachToRope(transform.position);

            if (pointMesh != null && activeMaterial != null)
                pointMesh.material = activeMaterial;

            AudioManager.Instance?.PlaySFX(null);

            EventBus.Publish(new HangingPointAttachedEvent
            {
                pointPosition = transform.position
            });
        }

        private void Detach()
        {
            if (_playerController != null)
                _playerController.DetachFromRope();

            _playerAttached = false;
            _isOccupied = false;

            if (pointMesh != null && inactiveMaterial != null)
                pointMesh.material = inactiveMaterial;

            EventBus.Publish(new HangingPointDetachedEvent
            {
                pointPosition = transform.position
            });
        }

        private void CheckForEnemyBelow()
        {
            if (!_playerAttached) return;

            Collider[] enemies = Physics.OverlapSphere(transform.position, enemyKillRange, _enemyLayer);

            foreach (var enemy in enemies)
            {
                Vector3 toEnemy = enemy.transform.position - transform.position;
                toEnemy.y = 0;

                if (toEnemy.magnitude <= enemyKillRange)
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        PerformRopeKill(enemy.gameObject);
                        return;
                    }
                }
            }
        }

        private void PerformRopeKill(GameObject enemy)
        {
            var enemyController = enemy.GetComponent<AI.EnemyController>();
            if (enemyController != null)
            {
                enemyController.ApplySleepEffect(20f);
                enemyController.TakeDamage(0);
            }

            EventBus.Publish(new RopeKillPerformedEvent
            {
                enemyId = enemy.GetInstanceID(),
                pointPosition = transform.position
            });
        }

        private void ShowPrompt(string message)
        {
            if (uiPrompt != null)
                uiPrompt.text = message;
        }

        public struct HangingPointAttachedEvent
        {
            public Vector3 pointPosition;
        }

        public struct HangingPointDetachedEvent
        {
            public Vector3 pointPosition;
        }

        public struct RopeKillPerformedEvent
        {
            public int enemyId;
            public Vector3 pointPosition;
        }
    }
}