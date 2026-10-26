using SharpGL;
using SharpGL.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CG3D
{
    class Handler
    {

        private OpenGL gl;
        private List<PolyhedronModel> drawables;
        private List<Key> pressedKeys;
        private Camera camera;
        private Vector2 mousePosition;
        private bool cameraCapture = false;
        public Handler(OpenGLControl glControl) 
        {
            camera = new Camera();
            pressedKeys = new List<Key>();
            drawables = new List<PolyhedronModel>();
            gl = glControl.OpenGL;
            gl.ClearColor(0.125f, 0.141f, 0.169f, 1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.ShadeModel(OpenGL.GL_SMOOTH);

            Test();
            GlobalLight();
        }


        public void Resized(int width , int height)
        {
            gl.Viewport(0, 0, width, height);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            gl.Perspective(
                45.0,
                (double)width / height,
                0.1,
                1000.0);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        public void AddKey(Key key) 
        {
            if (!pressedKeys.Contains(key))
            {
                if(key == Key.LeftShift) 
                {
                    camera.SpeedMultiplyer = 2;
                }
                pressedKeys.Add(key);
            }
        }

        public void RemoveKey(Key key) 
        {
            if (key == Key.LeftShift)
            {
                camera.SpeedMultiplyer = 1;
            }
            pressedKeys.Remove(key);
        }

        public void Deactovate() 
        {
            pressedKeys.Clear();
        }

        public void Update(float dt, Vector2 curMousePosition) 
        {
            Vector2 dMousePosition = curMousePosition - mousePosition;
            mousePosition = curMousePosition;
            UpdateCameraMovement(dt);
            if (cameraCapture) 
            {
                camera.CameraRotation(dMousePosition);
            }

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();



            float[] position = { 5f, 8f, 10f, 0f };
            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_POSITION,
                position);

            gl.Color(0.0f, 0.0f, 1.0f);
            draw();

        }

        public void CameraCapture(bool state) 
        {
            cameraCapture = state;
        }

        public void GlobalLight() 
        {
            gl.Enable(OpenGL.GL_LIGHTING);

            gl.Enable(OpenGL.GL_LIGHT0);

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_AMBIENT_AND_DIFFUSE);

            float[] globalAmbient = { 0.25f, 0.25f, 0.25f, 1f };
            gl.LightModel(
                OpenGL.GL_LIGHT_MODEL_AMBIENT,
                globalAmbient);


            float[] diffuse = { 0.8f, 0.8f, 0.8f, 1f };
            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_DIFFUSE,
                diffuse);


            float[] specular = { 0.5f, 0.5f, 0.5f, 1f };
            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_SPECULAR,
                specular);

            float[] position = { 5f, 8f, 10f, 0f };
            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_POSITION,
                position);

            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        

        private void draw() 
        {
            gl.Clear(
                OpenGL.GL_COLOR_BUFFER_BIT |
                OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            Vector3 target = camera.cameraPosition + camera.cameraFront;

            gl.LookAt(
                camera.cameraPosition.X,
                camera.cameraPosition.Y,
                camera.cameraPosition.Z,

                target.X,
                target.Y,
                target.Z,

                camera.cameraUp.X,
                camera.cameraUp.Y,
                camera.cameraUp.Z);

            foreach (var drawable in drawables) 
            {
                DrawPolyhedronModel(drawable);
            }

            gl.Flush();
        }
        private void UpdateCameraMovement(float deltaTime)
        {
            float velocity = camera.CameraSpeed * deltaTime;

            foreach (var key in pressedKeys) 
            {
                switch (key)
                {
                    case Key.W:
                        camera.ChangePosition(camera.cameraFront * velocity);
                        break;
                    case Key.S:
                        camera.ChangePosition(-camera.cameraFront * velocity);
                        break;
                    case Key.A:
                        camera.ChangePosition( -camera.cameraRight * velocity);
                        break;
                    case Key.D:
                        camera.ChangePosition( camera.cameraRight * velocity);
                        break;
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                        camera.ChangePosition( -camera.cameraUp * velocity);
                        break;
                    case Key.Space:
                        camera.ChangePosition( camera.cameraUp * velocity);
                        break;

                }
            
            
            }

        }

        private void DrawPolyhedronModel(PolyhedronModel model)
        {
            ReadOnlySpan<PolyhedronInstance> instances =
                model.Extrusion.Instances;

            ReadOnlySpan<Vector2> vertices =
                model.Polygon.Vertices;

            int instanceCount = instances.Length;
            int vertexCount = vertices.Length;

            if (instanceCount == 0 || vertexCount < 3)
                return;

            // Рисуем торцевые грани.
            DrawEndFace(model, 0, reverse: true);

            if (instanceCount > 1)
                DrawEndFace(model, instanceCount - 1, reverse: false);

            // Соединяем соседние копии боковыми гранями.
            for (int i = 0; i < instanceCount - 1; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    int nextVertex = (j + 1) % vertexCount;

                    Vector3 v0 = model.GetWorldVertex(i, j);
                    Vector3 v1 = model.GetWorldVertex(i, nextVertex);
                    Vector3 v2 = model.GetWorldVertex(i + 1, nextVertex);
                    Vector3 v3 = model.GetWorldVertex(i + 1, j);

                    Vector3 normal = Vector3.Normalize(Vector3.Cross(v1 - v0, v3 - v0));

                    gl.Begin(OpenGL.GL_QUADS);
                    gl.Normal(normal.X, normal.Y, normal.Z);

                    gl.Vertex(v0.X, v0.Y, v0.Z);
                    gl.Vertex(v1.X, v1.Y, v1.Z);
                    gl.Vertex(v2.X, v2.Y, v2.Z);
                    gl.Vertex(v3.X, v3.Y, v3.Z);

                    gl.End();


                    gl.Begin(OpenGL.GL_QUADS);

                    gl.Vertex(v0.X, v0.Y, v0.Z);
                    gl.Vertex(v1.X, v1.Y, v1.Z);
                    gl.Vertex(v2.X, v2.Y, v2.Z);
                    gl.Vertex(v3.X, v3.Y, v3.Z);

                    gl.End();
                }
            }
        }

        private void DrawEndFace(
            PolyhedronModel model,
            int instanceIndex,
            bool reverse)
        {
            ReadOnlySpan<Vector2> vertices = model.Polygon.Vertices;

            Vector3 normal = model.Extrusion.Instances[instanceIndex].Normal;

            if (reverse)
                normal = -normal;

            gl.Normal(normal.X, normal.Y, normal.Z);
            gl.Begin(OpenGL.GL_TRIANGLE_FAN);

            gl.Begin(OpenGL.GL_TRIANGLE_FAN);

            Vector3 center = model.Extrusion.Instances[instanceIndex].Offset;
            gl.Vertex(center.X, center.Y, center.Z);

            if (reverse)
            {
                for (int i = vertices.Length - 1; i >= 0; i--)
                {
                    Vector3 vertex = model.GetWorldVertex(instanceIndex, i);
                    gl.Vertex(vertex.X, vertex.Y, vertex.Z);
                }
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vector3 vertex = model.GetWorldVertex(instanceIndex, i);
                    gl.Vertex(vertex.X, vertex.Y, vertex.Z);
                }
            }

            Vector3 firstVertex = model.GetWorldVertex(
                instanceIndex,
                reverse ? vertices.Length - 1 : 0);

            gl.Vertex(firstVertex.X, firstVertex.Y, firstVertex.Z);

            gl.End();
        }

        private void Test() 
        {
            Polyhedron polygon = new Polyhedron(
            center: Vector2.Zero,
            radius: 1f,
            vertexCount: 5);
            Extrusion extrusion = new Extrusion(
                new PolyhedronInstance[]
                {
                    new(
                        new Vector3(0f, 0f, -3f),
                        Vector3.UnitZ),
                    new(
                        new Vector3(0f, 0f, 0f),
                        Vector3.UnitZ,
                        MathF.PI / 6f),
                    new(
                        new Vector3(1f, 1f, 3f),
                        Vector3.Normalize(new Vector3(1f, 0f, 1f)),
                        MathF.PI / 4f)
                });

            PolyhedronModel model = new PolyhedronModel(
                polygon,
                extrusion);
            drawables.Add(model);
        }


    }
}
