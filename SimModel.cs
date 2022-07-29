using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OrbitalSimWPF
{
    public class SimModel
    {

        ModelVisual3D ModelVisual3D { get; } = new();
        public Model3DGroup Model3DGroup { get; } = new();

        // Axes
        GeometryModel3D? X_AxisGeometryModel { get; set; }
        GeometryModel3D? Y_AxisGeometryModel { get; set; }
        GeometryModel3D? Z_AxisGeometryModel { get; set; }
        GeometryModel3D? X_AxisRefGeometryModel { get; set; }
        GeometryModel3D? Y_AxisRefGeometryModel { get; set; }
        GeometryModel3D? Z_AxisRefGeometryModel { get; set; }

        // Bodies
        public BodyList BodyList { get; set; }

        public SimModel() { }

        public void InitScene(System.Windows.Controls.Viewport3D simViewport
                        , BodyList bodyList)
        {

            InitAxes();

            InitBodies(bodyList);

            ModelVisual3D.Content = Model3DGroup;
            Model3DGroup.Children.Add(new AmbientLight(Colors.White));

            simViewport.Children.Add(ModelVisual3D);

        }

        /// <summary>
        /// Initialize geometry for the indiviual bodies
        /// </summary>
        /// <param name="bodyList"></param>
        private void InitBodies(BodyList bodyList)
        {
            BodyList = bodyList;

            foreach (Body b in BodyList.Bodies)
            {
                if (!b.Selected)
                    continue;

                if (!b.ID.Equals("10")) // Only Sol
                    continue;

                Model3DGroup.Children.Add(b.InitBody());
            }
        }

        /// <summary>
        /// Axes are long, rectngular prisims
        /// refpoints are spheres on positive side of each axis
        /// </summary>
        private void InitAxes(Boolean refPoints=true)
        {

            MeshGeometry3D AxisMesh = new();

            RotateTransform3D rotateTransform3D;
            QuaternionRotation3D quaternionRotation3D;

            double axisLength = Properties.Settings.Default.AxisLength; // 6E6D
            double axisWidth = 1E6 / 10 / 2;

            // *** X Axis
            /*
            // Collection of vertex positions
            Point3DCollection positions = new()
            {
                new Point3D(axisLength, axisWidth, axisWidth),     // 0
                new Point3D(axisLength, axisWidth, -axisWidth),    // 1
                new Point3D(axisLength, -axisWidth, -axisWidth),   // 2
                new Point3D(axisLength, -axisWidth, axisWidth),    // 3
                new Point3D(-axisLength, axisWidth, axisWidth),    // 4
                new Point3D(-axisLength, axisWidth, -axisWidth),   // 5
                new Point3D(-axisLength, -axisWidth, -axisWidth),  // 6
                new Point3D(-axisLength, -axisWidth, axisWidth)    // 7
            };
            AxisMesh.Positions = positions;

            // Collection of indices (12 triangles define rectangular box)
            Int32Collection triangleIndices = new()
            {
            // +X end
              0,3,2, 3,2,1
            // -X end
            , 4,5,7, 7,6,5
            // +Z side
            , 4,7,0, 7,3,0
            // -Z side
            , 5,1,6, 6,1,2
            // +Y side
            , 4,0,5, 5,0,1
            // -Y side
            , 7,6,3, 6,2,3
            };
            AxisMesh.TriangleIndices = triangleIndices;

            // Normals for each triangle
            Vector3DCollection normals = new()
            {
                // Calculate normals
                // +X side
                  MakeNormal(positions, triangleIndices, 0, 3, 2)
                , MakeNormal(positions, triangleIndices, 3, 2, 1)
                // -X side
                , MakeNormal(positions, triangleIndices, 4, 5, 7)
                , MakeNormal(positions, triangleIndices, 7, 6, 5)
                // +Z side
                , MakeNormal(positions, triangleIndices, 4, 7, 0)
                , MakeNormal(positions, triangleIndices, 7, 3, 0)
                // -Z side
                , MakeNormal(positions, triangleIndices, 5, 1, 6)
                , MakeNormal(positions, triangleIndices, 6, 1, 2)
                // *Y side
                , MakeNormal(positions, triangleIndices, 4, 0, 5)
                , MakeNormal(positions, triangleIndices, 5, 0, 1)
                // -Y side
                , MakeNormal(positions, triangleIndices, 7, 6, 3)
                , MakeNormal(positions, triangleIndices, 6, 2, 3)
            };
            AxisMesh.Normals = normals;
            */
            // Y, Z scale factors
            Double zfactor = Math.Sqrt(.75D) / 2D;
            Double yFactor = .5D / 2D;

            // Build up vertices of triangular prism
            Point3DCollection prismPositions = new()
            {
                new Point3D( axisLength/2D, 0D , zfactor*axisWidth),                 // 0 +X end
                new Point3D( axisLength/2D, -yFactor*axisWidth, -zfactor*axisWidth), // 1
                new Point3D( axisLength/2D, yFactor*axisWidth, -zfactor*axisWidth),  // 2
                new Point3D(-axisLength/2D, 0D , zfactor*axisWidth),                 // 3 -X end
                new Point3D(-axisLength/2D, -yFactor*axisWidth, -zfactor*axisWidth), // 4 
                new Point3D(-axisLength/2D, yFactor*axisWidth, -zfactor*axisWidth),  // 5
            };
            AxisMesh.Positions = prismPositions;

            // Collection of indices (12 triangles define surface of the prism)
            Int32Collection prismIndices = new()
            {
            // +X end  -X end
              0, 1, 2,   3, 5, 4
            // Sides
            , 0, 3, 4,   0, 4, 1
            , 3, 0, 2,   3, 2, 5
            , 1, 4, 5,   1, 5, 2
            };
            AxisMesh.TriangleIndices = prismIndices;

            // Normals for each triangle
            Vector3DCollection normals = new()
            {
                // Calculate normals
                // +X end
                  MakeNormal(prismPositions, prismIndices, 0, 1, 2)
                // -X end
                , MakeNormal(prismPositions, prismIndices, 3, 5, 4)
                // Sides
                , MakeNormal(prismPositions, prismIndices, 0, 3, 4)
                , MakeNormal(prismPositions, prismIndices, 0, 4, 1)
                , MakeNormal(prismPositions, prismIndices, 3, 0, 2)
                , MakeNormal(prismPositions, prismIndices, 3, 2, 5)
                , MakeNormal(prismPositions, prismIndices, 1, 4, 5)
                , MakeNormal(prismPositions, prismIndices, 1, 5, 2)
             };
            AxisMesh.Normals = normals;

            AxisMesh.TextureCoordinates = new PointCollection()
            {
                  new Point(0, 1)
                , new Point(1, 1)
                , new Point(1, 0)
                , new Point(0, 0)
            };

            // *** X Axis
            DiffuseMaterial diffuseMaterial = new(new SolidColorBrush(Colors.Black));
            X_AxisGeometryModel = new()
            {
                Geometry = AxisMesh,
                Material = diffuseMaterial
            };
            Model3DGroup.Children.Add(X_AxisGeometryModel);

            // Visual reference point for the positive end of the axis
            MeshGeometry3D? SphereMesh=null;
            if (refPoints)
            {
                SphereMesh = new();
                Sphere.AddSphere(SphereMesh, new(3.5E6D, 0D, 0D), 1E5D, 10, 10);
                X_AxisRefGeometryModel = new()
                {
                    Geometry = SphereMesh,
                    Material = diffuseMaterial
                };
                Model3DGroup.Children.Add(X_AxisRefGeometryModel);
            }

            // *** Y Axis
            diffuseMaterial = new(new SolidColorBrush(Colors.Blue));
            Y_AxisGeometryModel = new()
            {
                Geometry = AxisMesh,
                Material = diffuseMaterial
            };

            rotateTransform3D = new RotateTransform3D();
            quaternionRotation3D = new(new Quaternion(new Vector3D(0, 0, 1), 90));
            rotateTransform3D.Rotation = quaternionRotation3D;
            Y_AxisGeometryModel.Transform = rotateTransform3D;
            Model3DGroup.Children.Add(Y_AxisGeometryModel);

            // Visual reference point for the positive end of the axis
            if (refPoints)
            {
                Y_AxisRefGeometryModel = new()
                {
                    Geometry = SphereMesh,
                    Material = diffuseMaterial
                };
                Y_AxisRefGeometryModel.Transform = rotateTransform3D;
                Model3DGroup.Children.Add(Y_AxisRefGeometryModel);
            }

            // *** Z Axis
            diffuseMaterial = new(new SolidColorBrush(Colors.Yellow));
            Z_AxisGeometryModel = new()
            {
                Geometry = AxisMesh,
                Material = diffuseMaterial
            };

            rotateTransform3D = new RotateTransform3D();
            quaternionRotation3D = new(new Quaternion(new Vector3D(0, 1, 0), -90));
            rotateTransform3D.Rotation = quaternionRotation3D;
            Z_AxisGeometryModel.Transform = rotateTransform3D;
            Model3DGroup.Children.Add(Z_AxisGeometryModel);

            // Visual reference point for the positive end of the axis
            if (refPoints)
            {
                Z_AxisRefGeometryModel = new()
                {
                    Geometry = SphereMesh,
                    Material = diffuseMaterial
                };
                Z_AxisRefGeometryModel.Transform = rotateTransform3D;
                Model3DGroup.Children.Add(Z_AxisRefGeometryModel);
            }
        }

        /// <summary>
        /// Given two sides of triangle, compute Normal vector
        /// </summary>
        /// <param name="posCol">Vertex collection</param>
        /// <param name="iCol">Positions collection</param>
        /// <param name="i0">one vertex</param>
        /// <param name="i1">common vertex</param>
        /// <param name="i2">other vertex</param>
        /// <returns>Vector3D</returns>
        private static Vector3D MakeNormal(Point3DCollection posCol, Int32Collection iCol, int i0, int i1, int i2)
        {
            Vector3D n = Vector3D.CrossProduct(posCol[iCol[i0]] - posCol[iCol[i1]], posCol[iCol[i1]] - posCol[iCol[i2]]);
            n.Normalize();
            return n;
        }
    }
}
