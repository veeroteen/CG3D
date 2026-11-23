using SharpGL;
using SharpGL.SceneGraph.Assets;
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
    enum COLOROF
    {
        OBJECT,
        SOURCE,
        POINT

    }
    struct RGBAi 
    {
        public Vector4 RGBA;
        public COLOROF type;
        public int i;

        public RGBAi(Vector4 RGBA,int i,COLOROF type) 
        {
            this.type = type;
            this.i = i;
            this.RGBA = RGBA;
        }
    }

    class Handler
    {

        private OpenGL gl;
        private List<PolyhedronModel> drawables;
        private List<Key> pressedKeys;
        private Camera camera;
        private Vector2 mousePosition;
        private bool cameraCapture = false;
        private bool globalL = true;
        private bool pointL = false;
        private bool showWireframe = false;
        private bool showNormals = false;
        private bool projection = false;
        private Queue<RGBAi> colorqueue;
        private bool smoothing = true;
        private Texture texture;
        public Handler(OpenGLControl glControl) 
        {
            camera = new Camera();
            pressedKeys = new List<Key>();
            drawables = new List<PolyhedronModel>();
            colorqueue = new Queue<RGBAi>();
            gl = glControl.OpenGL;
            gl.ClearColor(0.125f, 0.141f, 0.169f, 1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            texture = new Texture();
            texture.Create(gl, new System.Drawing.Bitmap("texture.jpg"));
            Test();
            GlobalLight();
            PointLight();
            gl.Disable(OpenGL.GL_LIGHT1);
        }

        public void AddToColorQueue(RGBAi rGBAi) 
        {
            colorqueue.Enqueue(rGBAi);
        }
        public void ToggleProjection(int width, int height) 
        {
            projection = !projection;
            if (projection) 
            {
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                float aspectRatio = (float)width / height;
                float viewSize = 10f;

                gl.Ortho(
                    -viewSize * aspectRatio,
                     viewSize * aspectRatio,
                    -viewSize,
                     viewSize,
                     -100f,
                     100f);
            }
            else 
            {
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                gl.Perspective(
                    45.0,
                    (double)width / height,
                    0.1,
                    1000.0);

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
            }
        
        }
        public void Resized(int width, int height)
        {
            gl.Viewport(0, 0, width, height);
            if (projection)
            {
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                float aspectRatio = (float)width / height;
                float viewSize = 10f;

                gl.Ortho(
                    -viewSize * aspectRatio,
                     viewSize * aspectRatio,
                    -viewSize,
                     viewSize,
                     -100f,
                     100f);
            }
            else 
            {
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.LoadIdentity();

                gl.Perspective(
                    45.0,
                    (double)width / height,
                    0.1,
                    1000.0);

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
            }
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
            if (pressedKeys.Contains(key))
            {
                pressedKeys.Remove(key);
            }
        }

        public void Deactovate() 
        {
            pressedKeys.Clear();
        }

        public void Update(float dt, Vector2 curMousePosition) 
        {
            float[] constant = new float[1];
            float[] linear = new float[1];
            float[] quadratic = new float[1];

            gl.GetLight(OpenGL.GL_LIGHT1, OpenGL.GL_CONSTANT_ATTENUATION, constant);
            gl.GetLight(OpenGL.GL_LIGHT1, OpenGL.GL_LINEAR_ATTENUATION, linear);
            gl.GetLight(OpenGL.GL_LIGHT1, OpenGL.GL_QUADRATIC_ATTENUATION, quadratic);
            while (colorqueue.Count > 0) 
            {
                var elem = colorqueue.Dequeue();
                switch (elem.type) 
                {
                    case COLOROF.SOURCE:
                        switch (elem.i) 
                        {
                            case 0:
                                gl.Light(
                                    OpenGL.GL_LIGHT0,
                                    OpenGL.GL_AMBIENT,
                                    ToArray(elem.RGBA));
                                break;
                            case 1:
                                gl.Light(
                                    OpenGL.GL_LIGHT0,
                                    OpenGL.GL_DIFFUSE,
                                    ToArray(elem.RGBA));
                                break;
                            case 2:
                                gl.Light(
                                    OpenGL.GL_LIGHT0,
                                    OpenGL.GL_SPECULAR,
                                    ToArray(elem.RGBA));
                                break;
                        }
                        break;
                    case COLOROF.OBJECT:
                        if(drawables.Count > 0) 
                        {
                            drawables[0].ChangeMaterial(elem.RGBA, elem.i);
                        }
                        break;
                    case COLOROF.POINT:
                        switch (elem.i)
                        {
                            case 0:
                                gl.Light(
                                    OpenGL.GL_LIGHT1,
                                    OpenGL.GL_AMBIENT,
                                    ToArray(elem.RGBA));
                                break;
                            case 1:
                                gl.Light(
                                    OpenGL.GL_LIGHT1,
                                    OpenGL.GL_DIFFUSE,
                                    ToArray(elem.RGBA));
                                break;
                            case 2:
                                gl.Light(
                                    OpenGL.GL_LIGHT1,
                                    OpenGL.GL_SPECULAR,
                                    ToArray(elem.RGBA));
                                break;
                        }
                        break;
                }
            }



            Vector2 dMousePosition = curMousePosition - mousePosition;
            mousePosition = curMousePosition;
            UpdateCameraMovement(dt);
            if (cameraCapture) 
            {
                camera.CameraRotation(dMousePosition);
            }

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();



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


            float[] light =
            { 1.0f, 0.0f, 0.0f, 1.0f };
            gl.LightModel(
                OpenGL.GL_LIGHT_MODEL_AMBIENT,
                light);

            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_DIFFUSE,
                light);

            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_SPECULAR,
                light);

            float[] position = { 5f, 8f, 10f, 0f };
            gl.Light(
                OpenGL.GL_LIGHT0,
                OpenGL.GL_POSITION,
                position);

            gl.Enable(OpenGL.GL_NORMALIZE);
        }
        private void PointLight()
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT1);

            float[] ambient = { 1f, 1f, 1f, 1f };
            float[] diffuse = { 1f, 1f, 1f, 1f };
            float[] specular = { 1f, 1f, 1f, 1f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, diffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, specular);


            float[] position = { 0f, 10f, 0f, 1f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, position);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_CONSTANT_ATTENUATION, 1f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_LINEAR_ATTENUATION, 0.01f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_QUADRATIC_ATTENUATION, 0f);

        }

        private bool useTexture = false;

        public void ToggleTexture()
        {
            useTexture = !useTexture;
        }
        public void ToggleGlobalLight() 
        {
            if (globalL) 
            {
                gl.Disable(OpenGL.GL_LIGHT0);
                globalL = false;
            }
            else 
            {
                gl.Enable(OpenGL.GL_LIGHT0);
                globalL = true;
            }
        }
        public void ToggleWireframe()
        {
            showWireframe = !showWireframe;
        }
        public void ToggleSmoothing() 
        {
            smoothing = !smoothing;

            if (smoothing) 
            {
                gl.ShadeModel(OpenGL.GL_SMOOTH);
            }
            else
            {
                gl.ShadeModel(OpenGL.GL_FLAT);
            }
            
        }
        public void ToggleNormals()
        {
            showNormals = !showNormals;
        }
        public void TogglePointlLight()
        {
            if (pointL)
            {
                gl.Disable(OpenGL.GL_LIGHT1);
                pointL = false;
            }
            else
            {
                gl.Enable(OpenGL.GL_LIGHT1);
                pointL = true;
            }
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
            if (globalL)
            {
                float[] position = { 10f, 10f, 10f, 0f };
                gl.Light(
                    OpenGL.GL_LIGHT0,
                    OpenGL.GL_POSITION,
                    position);

                gl.Color(0.0f, 0.0f, 1.0f);
            }
            if (pointL)
            {
                float[] position = { 0f, 10f, 0f, 1f };
                gl.Light(
                    OpenGL.GL_LIGHT1,
                    OpenGL.GL_POSITION,
                    position);

                gl.Color(0.0f, 0.0f, 1.0f);
            }
            foreach (var drawable in drawables) 
            {
                DrawPolyhedronModel(drawable);
            }
            DrawTestPlane();
            gl.Flush();
        }
        private void UpdateCameraMovement(float deltaTime)
        {
            float velocity = camera.CameraSpeed * deltaTime;

            for (int i = 0; i < pressedKeys.Count;i++)
            {
                switch (pressedKeys[i])
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
                    case Key.H:
                        ToggleSmoothing();
                        RemoveKey(Key.H);
                        i--;
                        break;

                }
            
            
            }

        }
        private void DrawPolyhedronModel(PolyhedronModel model)
        {
            if (useTexture)
                DrawTexturedModel(model);
            else
                DrawSolidModel(model);
        }


        private void DrawSolidModel(PolyhedronModel model)
        {
            ReadOnlySpan<PolyhedronInstance> instances =
                model.Extrusion.Instances;

            ReadOnlySpan<Vector2> vertices =
                model.Polygon.Vertices;

            int instanceCount = instances.Length;
            int vertexCount = vertices.Length;

            if (instanceCount == 0 || vertexCount < 3)
                return;

            // Режим отображения полигонов.
            gl.PolygonMode(
                OpenGL.GL_FRONT_AND_BACK,
                showWireframe ? OpenGL.GL_LINE : OpenGL.GL_FILL);

            ApplyMaterial(model.Material);

            DrawEndFace(model, 0, reverse: true);

            if (instanceCount > 1)
                DrawEndFace(model, instanceCount - 1, reverse: false);

            for (int i = 0; i < instanceCount - 1; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    int nextVertex = (j + 1) % vertexCount;

                    Vector3 v0 = model.GetWorldVertex(i, j);
                    Vector3 v1 = model.GetWorldVertex(i, nextVertex);
                    Vector3 v2 = model.GetWorldVertex(i + 1, nextVertex);
                    Vector3 v3 = model.GetWorldVertex(i + 1, j);

                    Vector3 normal = Vector3.Normalize(
                        Vector3.Cross(v1 - v0, v3 - v0));

                    gl.Begin(OpenGL.GL_QUADS);
                    if (smoothing)
                    {
                        Vector3 n0 = GetVertexNormal(model, i, j);
                        Vector3 n1 = GetVertexNormal(model, i, nextVertex);
                        Vector3 n2 = GetVertexNormal(model, i + 1, nextVertex);
                        Vector3 n3 = GetVertexNormal(model, i + 1, j);

                        gl.Normal(n0.X, n0.Y, n0.Z);
                        gl.Vertex(v0.X, v0.Y, v0.Z);

                        gl.Normal(n1.X, n1.Y, n1.Z);
                        gl.Vertex(v1.X, v1.Y, v1.Z);

                        gl.Normal(n2.X, n2.Y, n2.Z);
                        gl.Vertex(v2.X, v2.Y, v2.Z);

                        gl.Normal(n3.X, n3.Y, n3.Z);
                        gl.Vertex(v3.X, v3.Y, v3.Z);
                    }
                    else
                    {
                        gl.Normal(normal.X, normal.Y, normal.Z);

                        gl.Vertex(v0.X, v0.Y, v0.Z);
                        gl.Vertex(v1.X, v1.Y, v1.Z);
                        gl.Vertex(v2.X, v2.Y, v2.Z);
                        gl.Vertex(v3.X, v3.Y, v3.Z);
                    }


                    gl.End();
                }
            }

            // Возвращаем стандартный режим, чтобы не затронуть другие объекты.
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            // Отрисовываем нормали.
            if (showNormals)
                DrawModelNormals(model);
        }
        private void DrawModelNormals(PolyhedronModel model)
        {
            ReadOnlySpan<PolyhedronInstance> instances =
                model.Extrusion.Instances;

            ReadOnlySpan<Vector2> vertices =
                model.Polygon.Vertices;

            int instanceCount = instances.Length;
            int vertexCount = vertices.Length;

            const float normalLength = 0.5f;

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Color(1f, 0f, 0f);

            // Торцевые нормали.
            DrawNormal(instances[0].Offset, -instances[0].Normal,normalLength);
            DrawFaceVertexNormals(model, 0, true, normalLength);

            if (instanceCount > 1)
            {
                DrawFaceVertexNormals(
                    model, instanceCount - 1, false, normalLength);
                DrawNormal(instances[instanceCount - 1].Offset, instances[instanceCount - 1].Normal,normalLength);
            }

            // Боковые нормали.
            gl.Begin(OpenGL.GL_LINES);

            for (int i = 0; i < instanceCount - 1; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    int nextVertex = (j + 1) % vertexCount;

                    Vector3 v0 = model.GetWorldVertex(i, j);
                    Vector3 v1 = model.GetWorldVertex(i, nextVertex);
                    Vector3 v2 = model.GetWorldVertex(i + 1, nextVertex);
                    Vector3 v3 = model.GetWorldVertex(i + 1, j);

                    Vector3 normal = Vector3.Normalize(
                        Vector3.Cross(v1 - v0, v3 - v0));

                    // Центр боковой грани.
                    Vector3 center = (v0 + v1 + v2 + v3) / 4f;

                    // Нормаль из центра полигона.
                    DrawNormal(center, normal, normalLength);
                    DrawNormal(v0, normal, normalLength);
                    DrawNormal(v1, normal, normalLength);
                    DrawNormal(v2, normal, normalLength);
                    DrawNormal(v3, normal, normalLength);
                }
            }

            gl.End();

            gl.Enable(OpenGL.GL_LIGHTING);
        }
        private void DrawNormal(Vector3 center, Vector3 normal, float length)
        {
            Vector3 end = center + normal * length;

            gl.Vertex(center.X, center.Y, center.Z);
            gl.Vertex(end.X, end.Y, end.Z);
        }

        private void DrawTexturedModel(PolyhedronModel model)
        {
            ReadOnlySpan<PolyhedronInstance> instances =
                model.Extrusion.Instances;

            ReadOnlySpan<Vector2> vertices =
                model.Polygon.Vertices;

            int instanceCount = instances.Length;
            int vertexCount = vertices.Length;

            if (instanceCount == 0 || vertexCount < 3)
                return;

            ApplyMaterial(model.Material);

            // Настраиваем режим отображения.
            gl.PolygonMode(
                OpenGL.GL_FRONT_AND_BACK,
                showWireframe ? OpenGL.GL_LINE : OpenGL.GL_FILL);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            texture.Bind(gl);

            // Торцевые грани.
            DrawTexturedEndFace(model, 0, reverse: true);

            if (instanceCount > 1)
                DrawTexturedEndFace(
                    model, instanceCount - 1, reverse: false);

            // Боковые грани.
            for (int i = 0; i < instanceCount - 1; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    int nextVertex = (j + 1) % vertexCount;

                    Vector3 v0 = model.GetWorldVertex(i, j);
                    Vector3 v1 = model.GetWorldVertex(i, nextVertex);
                    Vector3 v2 = model.GetWorldVertex(i + 1, nextVertex);
                    Vector3 v3 = model.GetWorldVertex(i + 1, j);

                    Vector3 normal = Vector3.Normalize(
                        Vector3.Cross(v1 - v0, v3 - v0));

                    float texU0 = (float)j / vertexCount;
                    float texU1 = (float)(j + 1) / vertexCount;

                    float texV0 = (float)i / (instanceCount - 1);
                    float texV1 = (float)(i + 1) / (instanceCount - 1);

                    gl.Begin(OpenGL.GL_QUADS);

                    gl.Normal(normal.X, normal.Y, normal.Z);

                    gl.TexCoord(texU0, texV0);
                    gl.Vertex(v0.X, v0.Y, v0.Z);

                    gl.TexCoord(texU1, texV0);
                    gl.Vertex(v1.X, v1.Y, v1.Z);

                    gl.TexCoord(texU1, texV1);
                    gl.Vertex(v2.X, v2.Y, v2.Z);

                    gl.TexCoord(texU0, texV1);
                    gl.Vertex(v3.X, v3.Y, v3.Z);

                    gl.End();
                }
            }

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.PolygonMode(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_FILL);

            // Нормали отображаем независимо от текстуры.
            if (showNormals)
                DrawModelNormals(model);
        }

        private void DrawTexturedEndFace(
            PolyhedronModel model,
            int instanceIndex,
            bool reverse)
        {
            ReadOnlySpan<Vector2> vertices = model.Polygon.Vertices;

            Vector3 normal = model.Extrusion.Instances[instanceIndex].Normal;

            if (reverse)
                normal = -normal;

            Vector2 min = vertices[0];
            Vector2 max = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                min = Vector2.Min(min, vertices[i]);
                max = Vector2.Max(max, vertices[i]);
            }

            Vector2 size = max - min;

            gl.Begin(OpenGL.GL_TRIANGLE_FAN);

            gl.Normal(normal.X, normal.Y, normal.Z);

            Vector3 center = model.Extrusion.Instances[instanceIndex].Offset;

            gl.TexCoord(
                (model.Polygon.Center.X - min.X) / size.X,
                (model.Polygon.Center.Y - min.Y) / size.Y);

            gl.Vertex(center.X, center.Y, center.Z);

            if (reverse)
            {
                for (int i = vertices.Length - 1; i >= 0; i--)
                    DrawTexturedFaceVertex(model, instanceIndex, i, min, size);
            }
            else
            {
                for (int i = 0; i < vertices.Length; i++)
                    DrawTexturedFaceVertex(model, instanceIndex, i, min, size);
            }

            int firstIndex = reverse ? vertices.Length - 1 : 0;

            DrawTexturedFaceVertex(
                model, instanceIndex, firstIndex, min, size);

            gl.End();
        }

        private void DrawTexturedFaceVertex(
            PolyhedronModel model,
            int instanceIndex,
            int vertexIndex,
            Vector2 min,
            Vector2 size)
        {
            Vector2 vertex2D = model.Polygon.Vertices[vertexIndex];

            Vector2 uv = new(
                (vertex2D.X - min.X) / size.X,
                (vertex2D.Y - min.Y) / size.Y);

            Vector3 vertex3D =
                model.GetWorldVertex(instanceIndex, vertexIndex);

            gl.TexCoord(uv.X, uv.Y);
            gl.Vertex(vertex3D.X, vertex3D.Y, vertex3D.Z);
        }
        private void DrawFaceVertexNormals(
            PolyhedronModel model,
            int instanceIndex,
            bool reverse,
            float length)
        {
            ReadOnlySpan<Vector2> vertices = model.Polygon.Vertices;

            Vector3 normal = model.Extrusion.Instances[instanceIndex].Normal;

            if (reverse)
                normal = -normal;

            gl.Begin(OpenGL.GL_LINES);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = model.GetWorldVertex(instanceIndex, i);
                Vector3 end = vertex + normal * length;

                gl.Vertex(vertex.X, vertex.Y, vertex.Z);
                gl.Vertex(end.X, end.Y, end.Z);
            }

            gl.End();
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

        private void ApplyMaterial(Material material)
        {
            gl.Material(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_AMBIENT,
                ToArray(material.Ambient));

            gl.Material(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_DIFFUSE,
                ToArray(material.Diffuse));

            gl.Material(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_SPECULAR,
                ToArray(material.Specular));

            gl.Material(
                OpenGL.GL_FRONT_AND_BACK,
                OpenGL.GL_SHININESS,
                material.Shininess);
        }

        private static float[] ToArray(Vector4 v)
        {
            return new[] { v.X, v.Y, v.Z, v.W };
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
                        0,
                        2f),
                    new(
                        new Vector3(1f, 1f, 3f),
                        Vector3.Normalize(new Vector3(1f, 0f, 1f)),
                        0,
                        2f
                        ),
                        new(
                        new Vector3(1f, 1f, 5f),
                        Vector3.Normalize(new Vector3(1f, 0f, 1f))
                        ),
                });
            Material blue = new(
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.8f, 1.0f),
                new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
                64f);
            PolyhedronModel model = new PolyhedronModel(
                polygon,
                extrusion,
                blue);
            drawables.Add(model);
        }

        private void DrawTestPlane(
            float size = 20f,
            int segments = 100,
            float height = -10f)
        {
            Material gray = new(
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f),
            64f);
            ApplyMaterial(gray);
            if (segments < 1 || size <= 0)
                return;

            float step = size / segments;
            float halfSize = size / 2f;

            gl.Begin(OpenGL.GL_TRIANGLES);

            // Нормаль направлена вверх
            gl.Normal(0f, 1f, 0f);

            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    float x0 = -halfSize + x * step;
                    float x1 = x0 + step;

                    float z0 = -halfSize + z * step;
                    float z1 = z0 + step;

                    // Первый треугольник
                    gl.Vertex(x0, height, z0);
                    gl.Vertex(x1, height, z1);
                    gl.Vertex(x1, height, z0);

                    // Второй треугольник
                    gl.Vertex(x0, height, z0);
                    gl.Vertex(x0, height, z1);
                    gl.Vertex(x1, height, z1);
                }
            }

            gl.End();
        }
        private Vector3 GetVertexNormal(
        PolyhedronModel model,
        int instanceIndex,
        int vertexIndex)
        {
            ReadOnlySpan<PolyhedronInstance> instances =
                model.Extrusion.Instances;

            ReadOnlySpan<Vector2> vertices =
                model.Polygon.Vertices;

            int instanceCount = instances.Length;
            int vertexCount = vertices.Length;

            Vector3 normal = Vector3.Zero;

            // Соседняя боковая грань слева от вершины.
            if (instanceIndex > 0)
            {
                int previousVertex = (vertexIndex - 1 + vertexCount) % vertexCount;

                Vector3 v0 = model.GetWorldVertex(instanceIndex - 1, previousVertex);
                Vector3 v1 = model.GetWorldVertex(instanceIndex - 1, vertexIndex);
                Vector3 v3 = model.GetWorldVertex(instanceIndex, previousVertex);

                normal += Vector3.Normalize(
                    Vector3.Cross(v1 - v0, v3 - v0));
            }

            // Соседняя боковая грань справа от вершины.
            if (instanceIndex < instanceCount - 1)
            {
                int nextVertex = (vertexIndex + 1) % vertexCount;

                Vector3 v0 = model.GetWorldVertex(instanceIndex, vertexIndex);
                Vector3 v1 = model.GetWorldVertex(instanceIndex, nextVertex);
                Vector3 v3 = model.GetWorldVertex(instanceIndex + 1, vertexIndex);

                normal += Vector3.Normalize(
                    Vector3.Cross(v1 - v0, v3 - v0));
            }

            if (normal.LengthSquared() < 1e-8f)
                return Vector3.UnitY;

            return Vector3.Normalize(normal);
        }

    }
}
