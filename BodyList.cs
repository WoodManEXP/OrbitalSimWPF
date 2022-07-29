using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace OrbitalSimWPF
{
    public class BodyList
    {

        public String SavedListCSV_File { get; }
        public String InitialListCSV_File { get; }
        public List<Body> Bodies { get; }

        public BodyList(String initialListCSV_File, String savedListCSV_File)
        {

            SavedListCSV_File = savedListCSV_File;
            InitialListCSV_File = initialListCSV_File;

            Bodies = new List<Body>();

            String savedListCSV_Path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), savedListCSV_File);

            // If the saved list file exists
            if (File.Exists(savedListCSV_Path))
            {
                LoadList(false);
            }
            else
            {
                LoadList(true);
            }
        }

        [JsonConstructor]
        public BodyList(String initialListCSV_File, String savedListCSV_File, List<Body> bodies)
        {

            SavedListCSV_File = savedListCSV_File;
            InitialListCSV_File = initialListCSV_File;
            Bodies = bodies;

        }

        public void LoadList(bool initialLoad)
        {

            String csvFileName = initialLoad ? InitialListCSV_File : SavedListCSV_File;

            // Use,InitSel,ID#,Name,Designation,IAU/aliases/other,Diameter,Mass,GM

            Bodies.Clear();

            // path to the csv file
            String csvPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), csvFileName);

            String[] csvBodies = System.IO.File.ReadAllLines(csvPath);
            foreach (String row in csvBodies)
            {
                String[] col = row.Split(',');

                if ("y".Equals(col[0])) // Entries with "y" here are available for sim (Has the effect of ignoring the header line)
                    Bodies.Add(new Body("y".Equals(col[1]), col[2], col[3], col[4], col[5], col[6], col[7], col[8]));
            }

        }

        internal int HowManySelected()
        {
            int i = 0;
            foreach (Body b in Bodies)
                if (b.Selected)
                    i++;
            return i;
        }

        public void SaveBodyList()
        {

            // path to the csv file
            String saveCSV_Path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), SavedListCSV_File);

            try
            {
                var writer = new StreamWriter(saveCSV_Path);

                writer.WriteLine("Use,InitSel,ID#,Name,Designation,IAU/aliases/other,Diameter,Mass,GM");

                foreach (Body b in Bodies)
                {
                    writer.WriteLine(b.ToCSV_String());
                }
                writer.Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Oops");
            }
        }

        public void SelectAll(Boolean bVal)
        {
            foreach (Body b in Bodies)
                b.Selected = bVal;
        }

        public void SetSelected(int n, Boolean selected)
        {
            Bodies.ElementAt(n).Selected = selected;
        }

        public int[] getSelected()
        {
            int[] selected = new int[HowManySelected()];

            int index = -1, i = -1;
            foreach (Body b in Bodies)
            {
                i++;
                if (b.Selected)
                    selected[++index] = i;
            }
            return selected;

        }

        /// <summary>
        /// Get current position of a body in the model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Point3D GetPosition(String name)
        {

            Point3D lookAtPoint;

            foreach (Body b in Bodies)
            {
                if (name.Equals(b.Name)) {
                    lookAtPoint = b.GetPosition();
                    break;
                }
            }
            return lookAtPoint;
        }
    }
}
