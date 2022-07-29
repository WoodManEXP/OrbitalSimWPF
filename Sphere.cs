using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

/*
 * http://csharphelper.com/howtos/howto_3D_sphere.html
 * http://csharphelper.com/howtos/howto_3D_smooth_sphere.html
 * 
 */
namespace OrbitalSimWPF
{
    public class Sphere
    {

        // Add a sphere.
        public static void AddSphere(MeshGeometry3D mesh, Point3D center,
            double radius, int num_phi, int num_theta)
        {
            double phi0, theta0;
            double dphi = Math.PI / num_phi;
            double dtheta = 2 * Math.PI / num_theta;

            Dictionary<Point3D, int> dictionary = new();

            phi0 = 0;
            double y0 = radius * Math.Cos(phi0);
            double r0 = radius * Math.Sin(phi0);
            for (int i = 0; i < num_phi; i++)
            {
                double phi1 = phi0 + dphi;
                double y1 = radius * Math.Cos(phi1);
                double r1 = radius * Math.Sin(phi1);

                // Point ptAB has phi value A and theta value B.
                // For example, pt01 has phi = phi0 and theta = theta1.
                // Find the points with theta = theta0.
                theta0 = 0;
                Point3D pt00 = new Point3D(
                    center.X + r0 * Math.Cos(theta0),
                    center.Y + y0,
                    center.Z + r0 * Math.Sin(theta0));
                Point3D pt10 = new Point3D(
                    center.X + r1 * Math.Cos(theta0),
                    center.Y + y1,
                    center.Z + r1 * Math.Sin(theta0));
                for (int j = 0; j < num_theta; j++)
                {
                    // Find the points with theta = theta1.
                    double theta1 = theta0 + dtheta;
                    Point3D pt01 = new Point3D(
                        center.X + r0 * Math.Cos(theta1),
                        center.Y + y0,
                        center.Z + r0 * Math.Sin(theta1));
                    Point3D pt11 = new Point3D(
                        center.X + r1 * Math.Cos(theta1),
                        center.Y + y1,
                        center.Z + r1 * Math.Sin(theta1));

                    // Create the triangles.
                    AddSmoothTriangle(mesh, dictionary, pt00, pt11, pt10);
                    AddSmoothTriangle(mesh, dictionary, pt00, pt01, pt11);

                    // Move to the next value of theta.
                    theta0 = theta1;
                    pt00 = pt01;
                    pt10 = pt11;
                }

                // Move to the next value of phi.
                phi0 = phi1;
                y0 = y1;
                r0 = r1;
            }
        }

        // Add a triangle to the indicated mesh.
        // Reuse points so triangles share normals.
        private static void AddSmoothTriangle(MeshGeometry3D mesh,
            Dictionary<Point3D, int> dict,
            Point3D point1, Point3D point2, Point3D point3)
        {
            int index1, index2, index3;

            // Find or create the points.
            if (dict.ContainsKey(point1)) index1 = dict[point1];
            else
            {
                index1 = mesh.Positions.Count;
                mesh.Positions.Add(point1);
                dict.Add(point1, index1);
            }

            if (dict.ContainsKey(point2)) index2 = dict[point2];
            else
            {
                index2 = mesh.Positions.Count;
                mesh.Positions.Add(point2);
                dict.Add(point2, index2);
            }

            if (dict.ContainsKey(point3)) index3 = dict[point3];
            else
            {
                index3 = mesh.Positions.Count;
                mesh.Positions.Add(point3);
                dict.Add(point3, index3);
            }

            // If two or more of the points are
            // the same, it's not a triangle.
            if ((index1 == index2) ||
                (index2 == index3) ||
                (index3 == index1)) return;

            // Create the triangle.
            mesh.TriangleIndices.Add(index1);
            mesh.TriangleIndices.Add(index2);
            mesh.TriangleIndices.Add(index3);
        }
    }
}
