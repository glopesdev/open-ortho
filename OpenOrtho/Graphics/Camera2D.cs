using System;
using OpenTK;

namespace OpenOrtho.Graphics
{
    public class Camera2D
    {
        public Camera2D()
        {
            Zoom = 1f;
        }

        public Vector2 Position { get; set; }

        public float Rotation { get; set; }

        public float Zoom { get; set; }

        public Matrix4 GetViewMatrix()
        {
            Matrix4 transform, temp;
            Matrix4.CreateRotationZ(-Rotation, out transform);

            Matrix4.CreateTranslation(-Position.X, -Position.Y, 0, out temp);
            Matrix4.Mult(ref transform, ref temp, out transform);

            temp = Matrix4.Scale(Zoom);
            Matrix4.Mult(ref transform, ref temp, out transform);
            return transform;
        }
    }
}

