using UnityEngine;

namespace CatBrotato.Wave
{
    public class ArenaManager : MonoBehaviour
    {
        [Header("Arena Bounds")]
        [SerializeField] private Rect arenaBounds = new Rect(-15f, -10f, 30f, 20f);

        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float cameraPadding = 1f;

        public Rect ArenaBounds => arenaBounds;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        /// <summary>
        /// Sets the arena bounds. Use this to change arena size per wave/stage.
        /// </summary>
        public void SetArenaBounds(Rect bounds)
        {
            arenaBounds = bounds;
        }

        /// <summary>
        /// Returns a random position on the edge of the arena, outside the camera view.
        /// Used for spawning enemies that approach from off-screen.
        /// </summary>
        public Vector2 GetRandomSpawnPosition()
        {
            Rect cameraRect = GetCameraWorldRect();

            // Pick a random edge (0=top, 1=bottom, 2=left, 3=right)
            int edge = Random.Range(0, 4);
            float x, y;

            switch (edge)
            {
                case 0: // Top edge
                    x = Random.Range(arenaBounds.xMin, arenaBounds.xMax);
                    y = Random.Range(cameraRect.yMax + cameraPadding, arenaBounds.yMax);
                    if (y < cameraRect.yMax + cameraPadding)
                        y = arenaBounds.yMax;
                    break;
                case 1: // Bottom edge
                    x = Random.Range(arenaBounds.xMin, arenaBounds.xMax);
                    y = Random.Range(arenaBounds.yMin, cameraRect.yMin - cameraPadding);
                    if (y > cameraRect.yMin - cameraPadding)
                        y = arenaBounds.yMin;
                    break;
                case 2: // Left edge
                    x = Random.Range(arenaBounds.xMin, cameraRect.xMin - cameraPadding);
                    if (x > cameraRect.xMin - cameraPadding)
                        x = arenaBounds.xMin;
                    y = Random.Range(arenaBounds.yMin, arenaBounds.yMax);
                    break;
                default: // Right edge
                    x = Random.Range(cameraRect.xMax + cameraPadding, arenaBounds.xMax);
                    if (x < cameraRect.xMax + cameraPadding)
                        x = arenaBounds.xMax;
                    y = Random.Range(arenaBounds.yMin, arenaBounds.yMax);
                    break;
            }

            // Clamp to arena bounds
            x = Mathf.Clamp(x, arenaBounds.xMin, arenaBounds.xMax);
            y = Mathf.Clamp(y, arenaBounds.yMin, arenaBounds.yMax);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Returns a random position anywhere inside the arena bounds.
        /// </summary>
        public Vector2 GetRandomPositionInArena()
        {
            float x = Random.Range(arenaBounds.xMin, arenaBounds.xMax);
            float y = Random.Range(arenaBounds.yMin, arenaBounds.yMax);
            return new Vector2(x, y);
        }

        /// <summary>
        /// Checks if a position is within the arena bounds.
        /// </summary>
        public bool IsInArena(Vector2 pos)
        {
            return arenaBounds.Contains(pos);
        }

        /// <summary>
        /// Clamps a position to stay within the arena bounds.
        /// </summary>
        public Vector2 ClampToArena(Vector2 pos)
        {
            pos.x = Mathf.Clamp(pos.x, arenaBounds.xMin, arenaBounds.xMax);
            pos.y = Mathf.Clamp(pos.y, arenaBounds.yMin, arenaBounds.yMax);
            return pos;
        }

        /// <summary>
        /// Clamps a transform's position to arena bounds. Useful for keeping the player inside.
        /// </summary>
        public void ClampTransformToArena(Transform target)
        {
            if (target == null) return;

            Vector3 pos = target.position;
            pos.x = Mathf.Clamp(pos.x, arenaBounds.xMin, arenaBounds.xMax);
            pos.y = Mathf.Clamp(pos.y, arenaBounds.yMin, arenaBounds.yMax);
            target.position = pos;
        }

        /// <summary>
        /// Gets the camera's visible area as a world-space Rect.
        /// </summary>
        private Rect GetCameraWorldRect()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                // Fallback: return a small rect at origin
                return new Rect(-5f, -3.5f, 10f, 7f);
            }

            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            Vector3 camPos = mainCamera.transform.position;

            return new Rect(
                camPos.x - cameraWidth * 0.5f,
                camPos.y - cameraHeight * 0.5f,
                cameraWidth,
                cameraHeight
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw arena bounds in green
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(
                arenaBounds.x + arenaBounds.width * 0.5f,
                arenaBounds.y + arenaBounds.height * 0.5f,
                0f
            );
            Vector3 size = new Vector3(arenaBounds.width, arenaBounds.height, 0.1f);
            Gizmos.DrawWireCube(center, size);

            // Draw camera bounds in yellow
            Gizmos.color = Color.yellow;
            Rect camRect = GetCameraWorldRect();
            Vector3 camCenter = new Vector3(
                camRect.x + camRect.width * 0.5f,
                camRect.y + camRect.height * 0.5f,
                0f
            );
            Vector3 camSize = new Vector3(camRect.width, camRect.height, 0.1f);
            Gizmos.DrawWireCube(camCenter, camSize);
        }
#endif
    }
}
