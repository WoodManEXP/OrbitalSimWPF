using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace OrbitalSimWPF
{
    public class SimCamera
    {

        public enum MoveDirection
        {
            MoveForward, MoveBackward, MoveUp, MoveDown, MoveLeft, MoveRight
        };

        public static PerspectiveCamera Camera { get; } = new()
        {
            FarPlaneDistance = 2.5E09,
            NearPlaneDistance = 0D
        };

        public Point3D Position { get { return Camera.Position; } set { Camera.Position = value; } }
        public Vector3D LookDirection { get { return Camera.LookDirection; } set { Camera.LookDirection = value; } }
        public Vector3D UpDirection { get { return Camera.UpDirection; } set { Camera.UpDirection = value; } }

        private static Duration UDLR_Duration { get; } = new Duration(TimeSpan.FromMilliseconds(500E0));
        private static Duration LookUDLR_Duration { get; } = new Duration(TimeSpan.FromMilliseconds(100E0));

        public Point3DAnimation Point3DAnimation { get; } = new()
        {
            AccelerationRatio = 0.3,
            DecelerationRatio = 0.3,
            Duration = UDLR_Duration,
            FillBehavior = FillBehavior.HoldEnd
        };
        public Vector3DAnimation Vector3DAnimationLD { get; } = new() // Look dir
        {
            AccelerationRatio = 0.3,
            DecelerationRatio = 0.5,
            Duration = LookUDLR_Duration,
            FillBehavior = FillBehavior.HoldEnd
        };
        public Vector3DAnimation Vector3DAnimationUD { get; } = new() // Up dir
        {
            AccelerationRatio = 0.3,
            DecelerationRatio = 0.5,
            Duration = LookUDLR_Duration,
            FillBehavior = FillBehavior.HoldEnd
        };

        private bool AnimatingPosn { get; set; }
        private bool AnimatingLD { get; set; }
        private bool AnimatingUD { get; set; }
        private Reticle Reticle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="simViewport"></param>
        /// <param name="cameraPosition"></param>
        /// <param name="lookDirection"></param>
        /// <param name="upDirection"></param>
        /// <param name="v"></param>
        public SimCamera(SimModel simModel, Point3D cameraPosition, double v)
        {
            Position = cameraPosition;
            Camera.FieldOfView = v;

            Reticle = new();
            simModel.Model3DGroup.Children.Add(Reticle.ReticleModel);

            Point3DAnimation.Completed += new EventHandler(AnimationCompletedPosn);
            Vector3DAnimationLD.Completed += new EventHandler(AnimationCompletedLD);
            Vector3DAnimationUD.Completed += new EventHandler(AnimationCompletedUD);

            AnimatingPosn = AnimatingLD = AnimatingUD = false;
        }

        public void LookAt(Point3D lookAtPoint)
        {
            // https://stackoverflow.com/questions/28656442/cameraposition-and-lookdirection-in-wpf
            Vector3D lookDirection = new(lookAtPoint.X - Position.X, lookAtPoint.Y - Position.Y, lookAtPoint.Z - Position.Z);
            lookDirection.Normalize();

            // Synthesize an upDir. Right hand rule, want Y coord to be positive (thumb up)
            Vector3D upDirection = new(0D, 1D, 0D);
            upDirection = Vector3D.CrossProduct(lookDirection, upDirection); // Initial upDir
            upDirection = Vector3D.CrossProduct(upDirection, lookDirection);
            upDirection.Normalize();

            LookDirection = lookDirection;
            UpDirection = upDirection;

            Reticle.PositionRecticle(this);
        }

        /// <summary>
        /// https://math.stackexchange.com/questions/1650877/how-to-find-a-point-which-lies-at-distance-d-on-3d-line-given-a-position-vector
        /// </summary>
        /// <param name="moveDirection"></param>
        public void Move(MoveDirection moveDirection)
        {

            if (AnimationActive())
                return;

            double f, d = 1E06D;
            Point3D posn = new(Position.X, Position.Y, Position.Z);

            long dX, dY, dZ;
            Point3D oPosn = new(Position.X, Position.Y, Position.Z);

            switch (moveDirection)
            {
                case MoveDirection.MoveForward:
                case MoveDirection.MoveBackward:
                    // Forward/Backwards on LookDirection unit vector
                    f = (MoveDirection.MoveBackward == moveDirection) ? -d : d;
                    posn.Offset(LookDirection.X * f, LookDirection.Y * f, LookDirection.Z * f);

                    dX = (long)(posn.X - oPosn.X);
                    dY = (long)(posn.Y - oPosn.Y);
                    dZ = (long)(posn.Z - oPosn.Z);
                    System.Diagnostics.Debug.WriteLine((moveDirection == MoveDirection.MoveForward) ? "Move: MoveForward" : "MoveBackward");
                    System.Diagnostics.Debug.WriteLine("Move: LookDir " + LookDirection);
                    System.Diagnostics.Debug.WriteLine("Move: UpDir " + UpDirection);
                    System.Diagnostics.Debug.WriteLine("Move: delta " + dX + " " + dY + " " + dZ);
                    System.Diagnostics.Debug.WriteLine("Move: posn " + (long)posn.X + " " + (long)posn.Y + " " + (long)posn.Z);
                    double distToOrigin = Math.Sqrt(posn.X * posn.X + posn.Y * posn.Y + posn.Z * posn.Z);
                    System.Diagnostics.Debug.WriteLine("Move: distToOrigin " + distToOrigin.ToString("#,##0"));
                    break;

                case MoveDirection.MoveUp:
                case MoveDirection.MoveDown:
                    // Up/Down the UpDirection unit vector
                    f = (MoveDirection.MoveDown == moveDirection) ? -d : d;
                    posn.Offset(UpDirection.X * f, UpDirection.Y * f, UpDirection.Z * f);

                    dX = (long)(posn.X - oPosn.X);
                    dY = (long)(posn.Y - oPosn.Y);
                    dZ = (long)(posn.Z - oPosn.Z);
                    //System.Diagnostics.Debug.WriteLine((moveDirection == MoveDirection.MoveUp) ? "Move: MoveUp" : "MoveDown");
                    //System.Diagnostics.Debug.WriteLine("Move: LookDir " + LookDirection);
                    //System.Diagnostics.Debug.WriteLine("Move: UpDir " + UpDirection);
                    //System.Diagnostics.Debug.WriteLine("Move: delta " + dX + " " + dY + " " + dZ);
                    //System.Diagnostics.Debug.WriteLine("Move: posn " + ((long)posn.X).ToString("#,##0") + " " + ((long)posn.Y).ToString("#,##0") + " " + ((long)posn.Z).ToString("#,##0"));
                    break;

                case MoveDirection.MoveLeft:
                case MoveDirection.MoveRight:
                    // Left/Right on cross product of LookDirection and UpDirection unit vectors
                    Vector3D lRVec = Vector3D.CrossProduct(LookDirection, UpDirection);
                    f = (MoveDirection.MoveRight == moveDirection) ? d : -d;
                    posn.Offset(lRVec.X * f, lRVec.Y * f, lRVec.Z * f);

                    dX = (long)(posn.X - oPosn.X);
                    dY = (long)(posn.Y - oPosn.Y);
                    dZ = (long)(posn.Z - oPosn.Z);
                    //System.Diagnostics.Debug.WriteLine((moveDirection == MoveDirection.MoveLeft) ? "Move: MoveLeft" : "MoveRight");
                    //System.Diagnostics.Debug.WriteLine("Move: lRVec " + lRVec.X + " " + lRVec.Y + " " + lRVec.Z);
                    //System.Diagnostics.Debug.WriteLine("Move: LookDir " + LookDirection);
                    //System.Diagnostics.Debug.WriteLine("Move: UpDir " + UpDirection);
                    //System.Diagnostics.Debug.WriteLine("Move: delta " + dX + " " + dY + " " + dZ);
                    //System.Diagnostics.Debug.WriteLine("Move: posn " + ((long)posn.X).ToString("#,##0") + " " + ((long)posn.Y).ToString("#,##0") + " " + ((long)posn.Z).ToString("#,##0"));
                    break;

                default:
                    break;
            }

            //Camera.Position = posn;
            //Reticle.PositionRecticle(this);

            // Animate from current to new posn
            Point3DAnimation.To = posn;
            AnimatingPosn = true;
            Camera.BeginAnimation(ProjectionCamera.PositionProperty, Point3DAnimation);
        }

        /// <summary>
        /// Alter look direction of Camera
        /// https://social.msdn.microsoft.com/Forums/en-US/080b45d1-f29b-415b-b1d6-39173185c0f1/rotate-vector-by-a-quaternion?forum=wpf
        /// </summary>
        /// <param name="lR_Theta">Theta degrees L(-) or R(+)</param>
        /// <param name="uD_Theta">Theta degrees U(+) or D(-)</param>
        public void Look(Double lR_Theta, Double uD_Theta)
        {

            if (AnimationActive())
                return;

            Matrix3D m = Matrix3D.Identity;

            // L,R about Camera's UpDirection
            Quaternion q = new Quaternion(UpDirection, lR_Theta);

            // U,D about vector crossproduct of LookDirection and UpDiretion
            Vector3D uD_Vector = Vector3D.CrossProduct(LookDirection, UpDirection);
            Quaternion r = new Quaternion(uD_Vector, uD_Theta);

            // Combine L,R U,D rotations
            Quaternion s = Quaternion.Multiply(q, r);

            m.Rotate(s);

            Vector3DAnimationLD.To = m.Transform(LookDirection);
            Vector3DAnimationUD.To = m.Transform(UpDirection);

            //LookDirection = m.Transform(LookDirection);
            //UpDirection = m.Transform(UpDirection);
            //Reticle.PositionRecticle(this);

            //System.Diagnostics.Debug.WriteLine("Look: lR_Theta " + lR_Theta + " uD_Theta " + uD_Theta + " LookDir " + Vector3DAnimationLD);
            //System.Diagnostics.Debug.WriteLine("Look: UpDir " + Vector3DAnimationUD);

            AnimatingUD = AnimatingLD = true;
            Camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, Vector3DAnimationLD);
            Camera.BeginAnimation(ProjectionCamera.UpDirectionProperty, Vector3DAnimationUD);

        }
        /// <summary>
        /// Any camera animations active
        /// </summary>
        /// <returns></returns>
        private bool AnimationActive()
        {
            return AnimatingPosn || AnimatingLD || AnimatingUD;
        }
        private void AnimationCompletedPosn(object? sender, EventArgs e)
        {
            AnimatingPosn = false;
            Reticle.PositionRecticle(this);
        }
        private void AnimationCompletedLD(object? sender, EventArgs e)
        {
            AnimatingLD = false;
            Reticle.PositionRecticle(this);
        }
        private void AnimationCompletedUD(object? sender, EventArgs e)
        {
            AnimatingUD = false;
            Reticle.PositionRecticle(this);
        }
    }
}
