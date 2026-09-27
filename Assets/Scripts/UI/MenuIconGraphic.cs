using UnityEngine;
using UnityEngine.UI;

namespace MergeStudio.UI
{
    // Vector UI symbols avoid device/font-specific missing glyphs.
    public sealed class MenuIconGraphic : MaskableGraphic
    {
        public enum Symbol { Settings, Sound, Information }
        public Symbol Kind;
        private bool _muted;
        public bool Muted
        {
            get => _muted;
            set { _muted = value; SetVerticesDirty(); }
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (Kind == Symbol.Information)
            {
                Line(mesh, new Vector2(0.5f, 0.20f), new Vector2(0.5f, 0.62f), 0.16f);
                Ring(mesh, new Vector2(0.5f, 0.82f), 0.085f, 0.085f);
            }
            else if (Kind == Symbol.Settings)
            {
                Ring(mesh, new Vector2(0.5f, 0.5f), 0.30f, 0.13f);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI / 4f;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Line(mesh, Vector2.one * 0.5f + direction * 0.27f, Vector2.one * 0.5f + direction * 0.44f, 0.15f);
                }
            }
            else
            {
                Quad(mesh, new Vector2(0.09f, 0.37f), new Vector2(0.31f, 0.37f), new Vector2(0.31f, 0.63f), new Vector2(0.09f, 0.63f));
                Quad(mesh, new Vector2(0.28f, 0.37f), new Vector2(0.55f, 0.15f), new Vector2(0.55f, 0.85f), new Vector2(0.28f, 0.63f));
                if (_muted)
                {
                    Line(mesh, new Vector2(0.67f, 0.33f), new Vector2(0.93f, 0.67f), 0.08f);
                    Line(mesh, new Vector2(0.67f, 0.67f), new Vector2(0.93f, 0.33f), 0.08f);
                }
                else
                {
                    Arc(mesh, 0.31f);
                    Arc(mesh, 0.49f);
                }
            }
        }

        private void Arc(VertexHelper mesh, float radius)
        {
            for (int i = 0; i < 12; i++)
            {
                float a = Mathf.Lerp(-0.75f, 0.75f, i / 12f);
                float b = Mathf.Lerp(-0.75f, 0.75f, (i + 1) / 12f);
                Vector2 center = new Vector2(0.43f, 0.5f);
                Line(mesh, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    center + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, 0.055f);
            }
        }

        private void Ring(VertexHelper mesh, Vector2 center, float radius, float thickness)
        {
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI / 16f, b = (i + 1) * Mathf.PI / 16f;
                Vector2 u = new Vector2(Mathf.Cos(a), Mathf.Sin(a)), v = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                Quad(mesh, center + u * radius, center + v * radius,
                    center + v * (radius - thickness), center + u * (radius - thickness));
            }
        }

        private void Line(VertexHelper mesh, Vector2 a, Vector2 b, float width)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 side = new Vector2(-direction.y, direction.x) * width * 0.5f;
            Quad(mesh, a - side, b - side, b + side, a + side);
        }

        private void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            Rect rect = GetPixelAdjustedRect();
            int first = mesh.currentVertCount;
            foreach (Vector2 p in new[] { a, b, c, d })
                mesh.AddVert(new Vector3(rect.xMin + p.x * rect.width, rect.yMin + p.y * rect.height), color, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
