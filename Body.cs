using System;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OrbitalSimWPF
{
    public class Body
    {
        public Boolean Selected { get; set; }
        public String ID { get; }
        public String Name { get; }
        public String Designation { get; }
        public String IAU_Alias { get; }
        public String MassStr { get; }
        public String DiameterStr { get; }
        public String GM_Str { get; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double VX { get; set; }
        public double VY { get; set; }
        public double VZ { get; set; }
        public double LT { get; set; }
        public double RG { get; set; }
        public double RR { get; set; }
        public double Diameter { get; set; }
        public double Mass { get; set; }
        public double GM { get; set; }

        static private MeshGeometry3D SharedBodySphereMesh = null;
        private Transform3DGroup Transform3DGroup { get; } = new();

        public Body(Boolean selected       /* 1 */
                    , String id            /* 2 */
                    , String name          /* 3 */
                    , String designation   /* 4 */
                    , String iAU_Alias     /* 5 */
                    , String diameteStr    /* 6 */
                    , String massStr       /* 7 */
                    , String gM_Str)       /* 8 */
        {

            Selected = selected;
            ID = id;
            Name = name;
            Designation = designation;
            IAU_Alias = iAU_Alias;
            DiameterStr = diameteStr;
            MassStr = massStr;
            GM_Str = gM_Str;

            // Diameter, Mass, and GM values to double

            double dVal;
            Diameter = double.TryParse(diameteStr, out dVal) ? dVal : -1D;
            Mass = double.TryParse(massStr, out dVal) ? dVal : -1D;
            GM = double.TryParse(GM_Str, out dVal) ? dVal : -1D;

            FinishConstruct();

        }

        [JsonConstructor]
        public Body(Boolean selected        /* 1 */
                    , String id             /* 2 */
                    , String name           /* 3 */
                    , String designation    /* 4 */
                    , String iAU_Alias      /* 5 */
                    , String diameterStr       /* 6 */
                    , String massStr        /* 7 */
                    , String gM_Str
                    , double x, double y, double z
                    , double vX, double vY, double vZ
                    , double lT, double rG, double rR
                    , double diameter, double mass, double gM)
        {

            Selected = selected;
            ID = id;
            Name = name;
            Designation = designation;
            IAU_Alias = iAU_Alias;
            DiameterStr = diameterStr;
            MassStr = massStr;
            GM_Str = gM_Str;
            X = x; Y = y; Z = z;
            VX = vX; VY = vY; VZ = vZ;
            LT = lT; rG = RG; RR = rR;
            Diameter = diameter;
            Mass = mass;
            GM = gM;

            FinishConstruct();
        }

        private void FinishConstruct()
        {
            // Build the share sphere
            if (null == SharedBodySphereMesh)
            {
                SharedBodySphereMesh = new();
                Sphere.AddSphere(SharedBodySphereMesh, new(0D, 0D, 0D), 1, 10, 10);
            }
        }

        public GeometryModel3D InitBody()
        {
            TranslateTransform3D translateTransform3D = new(new(X, Y, Z));
            ScaleTransform3D scaleTransform3D = new(new(Diameter, Diameter, Diameter));

            Transform3DGroup.Children.Add(translateTransform3D);
            Transform3DGroup.Children.Add(scaleTransform3D);

            DiffuseMaterial diffuseMaterial = new(new SolidColorBrush(Colors.Yellow));

            GeometryModel3D geometryModel = new()
            {
                Geometry = SharedBodySphereMesh,
                Material = diffuseMaterial
            };

            geometryModel.Transform = Transform3DGroup;

            return geometryModel;
        }

        /// <summary>
        /// Get curreent position of a body (x, y, z)
        /// </summary>
        /// <returns></returns>
        public Point3D GetPosition()
        {
            double x = 0D, y = 0D, z = 0D;

            foreach (Transform3D transform in Transform3DGroup.Children)
            {
                if ("TranslateTransform3D".Equals(transform.DependencyObjectType.Name))
                {
                    x = ((TranslateTransform3D)transform).OffsetX;
                    y = ((TranslateTransform3D)transform).OffsetY;
                    z = ((TranslateTransform3D)transform).OffsetZ;
                }
            }
            return new(x, y, z);
        }
        public String ToCSV_String()
        {

            // Use,InitSel,ID#,Name,Designation,IAU/aliases/other,Diameter,Mass,GM

            String comma = ",";

            // Bodies.Add(new Body("y".Equals(col[1]), col[2], col[3], col[4], col[5], col[6], col[7], col[8]));
            return new String("y" + comma + (Selected ? "y" : "n") + comma + ID + comma + Name + comma + Designation
                + comma + IAU_Alias + comma + DiameterStr + comma + MassStr + comma + GM_Str);

        }
    }
}
