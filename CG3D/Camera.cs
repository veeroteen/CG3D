using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CG3D
{


    internal class Camera
    {
        public Quaternion cameraRotation { get; private set; }
        public Vector3 cameraFront { get; private set; }
        public Vector3 cameraRight { get; private set; }
        public Vector3 cameraUp { get; private set; }
        public float BaseCameraSpeed { get; private set; }
        public float SpeedMultiplyer { get; set; }

        public float CameraSpeed => BaseCameraSpeed * SpeedMultiplyer; 

        public float MouseSensitivity { get; private set; }

        public Vector3 cameraPosition { get; private set; }
        public Camera()
        {
            cameraRotation = Quaternion.Identity;
            cameraFront = -Vector3.UnitZ;
            cameraRight = Vector3.UnitX;
            cameraUp = Vector3.UnitY;
            BaseCameraSpeed = 4f;
            SpeedMultiplyer = 1f;
            MouseSensitivity = 0.15f;
            cameraPosition =  new(0f, 0f, 7f);
        }

        public void ChangePosition(Vector3 shift) 
        {
            cameraPosition += shift;
        }

        private void UpdateCameraVectors()
        {
            cameraFront = Vector3.Normalize(
                Vector3.Transform(-Vector3.UnitZ, cameraRotation));

            cameraRight = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitX, cameraRotation));

            cameraUp = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitY, cameraRotation));
        }

        public void CameraRotation(Vector2 dm)
        {

            Quaternion yawRotation = Quaternion.CreateFromAxisAngle(
                Vector3.UnitY,
                -dm.X * MouseSensitivity * MathF.PI / 180f);

            cameraRotation = Quaternion.Normalize(
                yawRotation * cameraRotation);


            cameraRight = Vector3.Normalize(
                Vector3.Transform(Vector3.UnitX, cameraRotation));

            Quaternion pitchRotation = Quaternion.CreateFromAxisAngle(
                cameraRight,
                -dm.Y * MouseSensitivity * MathF.PI / 180f);

            cameraRotation = Quaternion.Normalize(
                pitchRotation * cameraRotation);

            UpdateCameraVectors();
        }


    }
}
