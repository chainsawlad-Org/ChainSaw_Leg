using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ChainSawLeg.Application.Editor
{
    internal static class Collider2DGizmoDrawer
    {
        private const int InteractionLayerIndex = 6;

        private static readonly Color PhysicsColliderColor =
            new Color(0.1f, 0.8f, 1f, 0.95f);

        private static readonly Color InteractionColliderColor =
            new Color(1f, 0.6f, 0.05f, 0.95f);

        private static readonly Color TriggerColliderColor =
            new Color(1f, 0.15f, 0.7f, 0.95f);

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawBoxCollider(BoxCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawBox(collider);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawCircleCollider(CircleCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawCircle(collider);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawPolygonCollider(PolygonCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawPolygon(collider);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawEdgeCollider(EdgeCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawEdge(collider);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawCapsuleCollider(CapsuleCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawLocalBounds(collider, collider.offset, collider.size);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawCompositeCollider(CompositeCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawBounds(collider);
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawTilemapCollider(TilemapCollider2D collider, GizmoType gizmoType)
        {
            Prepare(collider);
            DrawBounds(collider);
        }

        private static void Prepare(Collider2D collider)
        {
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = ResolveColor(collider);
        }

        private static Color ResolveColor(Collider2D collider)
        {
            if (collider.isTrigger)
            {
                return TriggerColliderColor;
            }

            return collider.gameObject.layer == InteractionLayerIndex
                ? InteractionColliderColor
                : PhysicsColliderColor;
        }

        private static void DrawBox(BoxCollider2D collider)
        {
            DrawLocalBounds(collider, collider.offset, collider.size);
        }

        private static void DrawCircle(CircleCollider2D collider)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = collider.transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(collider.offset, collider.radius);
            Gizmos.matrix = previousMatrix;
        }

        private static void DrawPolygon(PolygonCollider2D collider)
        {
            for (int pathIndex = 0; pathIndex < collider.pathCount; pathIndex++)
            {
                Vector2[] points = collider.GetPath(pathIndex);
                DrawPath(collider.transform, points, closePath: true);
            }
        }

        private static void DrawEdge(EdgeCollider2D collider)
        {
            DrawPath(collider.transform, collider.points, closePath: false);
        }

        private static void DrawPath(Transform transform, Vector2[] points, bool closePath)
        {
            if (points == null || points.Length < 2)
            {
                return;
            }

            for (int index = 1; index < points.Length; index++)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(points[index - 1]),
                    transform.TransformPoint(points[index]));
            }

            if (closePath)
            {
                Gizmos.DrawLine(
                    transform.TransformPoint(points[points.Length - 1]),
                    transform.TransformPoint(points[0]));
            }
        }

        private static void DrawBounds(Collider2D collider)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            Gizmos.matrix = previousMatrix;
        }

        private static void DrawLocalBounds(
            Collider2D collider,
            Vector2 center,
            Vector2 size)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = collider.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
            Gizmos.matrix = previousMatrix;
        }
    }
}
