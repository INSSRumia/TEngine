using UnityEngine;
namespace GameLogic.Gameplay.Combat
{
    public class Battlefield : MonoBehaviour
    {
        public Bounds PlayerSpawnBounds = new Bounds(new Vector3(-5f, 0f, 0f), new Vector3(3f, 0f, 3f));
        public Bounds EnemySpawnBounds = new Bounds(new Vector3(5f, 0f, 0f), new Vector3(3f, 0f, 3f));
        public int PlayerSide = 1;
        public int EnemySide = 2;
        public Color PlayerSpawnGizmoColor = new Color(0.2f, 0.65f, 1f, 0.25f);
        public Color EnemySpawnGizmoColor = new Color(1f, 0.25f, 0.2f, 0.25f);

        public void PlaceMarble(Marble.Marble marble, int side)
        {
            if (marble == null)
                return;

            var bounds = side == EnemySide ? EnemySpawnBounds : PlayerSpawnBounds;
            marble.transform.position = transform.TransformPoint(GetRandomPoint(bounds));
        }

        private static Vector3 GetRandomPoint(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            var localPoint = new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z));
            return localPoint;
        }

        private void OnDrawGizmosSelected()
        {
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            DrawBounds(PlayerSpawnBounds, PlayerSpawnGizmoColor);
            DrawBounds(EnemySpawnBounds, EnemySpawnGizmoColor);
            Gizmos.matrix = oldMatrix;
        }

        private static void DrawBounds(Bounds bounds, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawCube(bounds.center, bounds.size);
            Gizmos.color = new Color(color.r, color.g, color.b, 1f);
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}