using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace OrbitalSimWPF
{
    public class Reticle
    {
        Model3DGroup Model3DGroup { get; set; }
        public GeometryModel3D ReticleModel { get; set; }
        private Point3D CurrLocation = new(), NewLocation = new();
        private TranslateTransform3D TranslateTransform3D { get; } = new();

        public DoubleAnimation DoubleAnimation { get; } = new()
        {
            AccelerationRatio = 0.3,
            DecelerationRatio = 0.3,
            Duration = new Duration(TimeSpan.FromMilliseconds(100E0)),
            FillBehavior = FillBehavior.HoldEnd
        };

        public Reticle(Model3DGroup model3DGroup)
        {

            Model3DGroup = model3DGroup;

            CurrLocation.X = 0D;
            CurrLocation.Y = 0D;
            CurrLocation.Z = 0D;

            // Reticle model
            MeshGeometry3D SphereMesh = new();
            Sphere.AddSphere(SphereMesh, CurrLocation, 1D, 10, 10);
            ReticleModel = new()
            {
                Geometry = SphereMesh,
                Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red))
            };

            ReticleModel.Transform = TranslateTransform3D;
            Model3DGroup.Children.Add(ReticleModel);
        }

        /// <summary>
        /// Animate reticle to new location
        /// </summary>
        /// <param name="posn">Of camera</param>
        /// <param name="lookVector">Of camera (unit vector)</param>
        public void PositionRecticle(SimCamera camera)
        {
            const double distFromCamera = 100D;

            //NewLocation.X = camera.Position.X + (distFromCamera * camera.LookDirection.X);
            //NewLocation.Y = camera.Position.Y + (distFromCamera * camera.LookDirection.Y);
            //NewLocation.Z = camera.Position.Z + (distFromCamera * camera.LookDirection.Z);

            NewLocation = camera.Position + (distFromCamera * camera.LookDirection);

            // Reposition reticle
            TranslateTransform3D.OffsetX = NewLocation.X; // - CurrLocation.X;
            TranslateTransform3D.OffsetY = NewLocation.Y; // - CurrLocation.Y;
            TranslateTransform3D.OffsetZ = NewLocation.Z; // - CurrLocation.Z;

            //long dX, dY, dZ;
            //dX = (long)(NewLocation.X - CurrLocation.X);
            //dY = (long)(NewLocation.Y - CurrLocation.Y);
            //dZ = (long)(NewLocation.Z - CurrLocation.Z);
            //System.Diagnostics.Debug.WriteLine("PositionRecticle: LookDir " + camera.LookDirection);
            //System.Diagnostics.Debug.WriteLine("PositionRecticle: camera posn " + camera.Position);
            //System.Diagnostics.Debug.WriteLine("PositionRecticle: rectile posn " + NewLocation);
            //System.Diagnostics.Debug.WriteLine("PositionRecticle: delta " + dX + " " + dY + " " + dZ);

            CurrLocation = NewLocation;

            //TranslateTransform3D.BeginAnimation(TranslateTransform3D.OffsetXProperty, DoubleAnimation);
            //TranslateTransform3D.BeginAnimation(TranslateTransform3D.OffsetYProperty, DoubleAnimation);
            //TranslateTransform3D.BeginAnimation(TranslateTransform3D.OffsetZProperty, DoubleAnimation);

        }
    }
}
