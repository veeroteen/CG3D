using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CG3D
{

    class Polyhedron
    {
        private readonly Vector2[] vertices;

        public Vector2 Center { get; }

        public ReadOnlySpan<Vector2> Vertices => vertices;

        public Polyhedron(
            Vector2 center,
            float radius,
            int vertexCount,
            float startAngle = -MathF.PI / 2f)
        {
            if (vertexCount < 3)
                throw new ArgumentOutOfRangeException(
                    nameof(vertexCount));

            if (radius <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(radius));

            Center = center;
            vertices = new Vector2[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                float angle = startAngle +
                    2f * MathF.PI * i / vertexCount;

                vertices[i] = new Vector2(
                    center.X + radius * MathF.Cos(angle),
                    center.Y + radius * MathF.Sin(angle));
            }
        }
    }

    struct PolyhedronInstance
    {
        public Vector3 Offset { get; }
        public Vector3 Normal { get; }
        public float RotationAroundNormal { get; }

        public PolyhedronInstance(
            Vector3 offset,
            Vector3 normal,
            float rotationAroundNormal = 0f)
        {
            if (normal.LengthSquared() < 1e-12f)
                throw new ArgumentException(
                    "Нормаль не может быть нулевым вектором.",
                    nameof(normal));

            Offset = offset;
            Normal = Vector3.Normalize(normal);
            RotationAroundNormal = rotationAroundNormal;
        }
    }

    class Extrusion
    {
        private readonly PolyhedronInstance[] instances;

        public ReadOnlySpan<PolyhedronInstance> Instances =>
            instances;

        public Extrusion(PolyhedronInstance[] instances)
        {
            ArgumentNullException.ThrowIfNull(instances);

            this.instances = (PolyhedronInstance[])instances.Clone();
        }
    }



    class PolyhedronModel
    {
        public Polyhedron Polygon { get; }
        public Extrusion Extrusion { get; }

        public PolyhedronModel(
            Polyhedron polygon,
            Extrusion extrusion)
        {
            ArgumentNullException.ThrowIfNull(polygon);
            ArgumentNullException.ThrowIfNull(extrusion);

            Polygon = polygon;
            Extrusion = extrusion;
        }

        public Vector3 GetWorldVertex(
            int instanceIndex,
            int vertexIndex)
        {
            PolyhedronInstance instance =
                Extrusion.Instances[instanceIndex];

            Vector2 vertex = Polygon.Vertices[vertexIndex];

            Vector3 localVertex = new(
                vertex.X - Polygon.Center.X,
                vertex.Y - Polygon.Center.Y,
                0f);

            Quaternion rotationAroundNormal =
                Quaternion.CreateFromAxisAngle(
                    Vector3.UnitZ,
                    instance.RotationAroundNormal);

            localVertex = Vector3.Transform(
                localVertex,
                rotationAroundNormal);


            Quaternion planeRotation = FromToRotation(
                Vector3.UnitZ,
                instance.Normal);

            Vector3 rotatedVertex = Vector3.Transform(
                localVertex,
                planeRotation);

            return rotatedVertex + instance.Offset;
        }

        private static Quaternion FromToRotation(
            Vector3 from,
            Vector3 to)
        {
            from = Vector3.Normalize(from);
            to = Vector3.Normalize(to);

            float dot = Vector3.Dot(from, to);

            if (dot < -0.999999f)
            {
                Vector3 axis = Vector3.Cross(
                    from, Vector3.UnitX);

                if (axis.LengthSquared() < 1e-6f)
                {
                    axis = Vector3.Cross(
                        from, Vector3.UnitY);
                }

                return Quaternion.CreateFromAxisAngle(
                    Vector3.Normalize(axis),
                    MathF.PI);
            }

            Vector3 cross = Vector3.Cross(from, to);

            return Quaternion.Normalize(
                new Quaternion(cross, 1f + dot));
        }
    }


}
