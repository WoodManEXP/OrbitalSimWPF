using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Text.Json;

namespace OrbitalSimWPF
{
    /// <summary>
    /// Interaction logic for EphemerisReader.xaml
    /// </summary>
    /// 

    // https://stackoverflow.com/questions/4019831/how-do-you-center-your-main-window-in-wpf

    public partial class EphemerisReader : Window
    {
        public enum ReadSerialized { Serialized, FromJPL };
        private BodyList BodyList;

        private readonly Double JPL_G = 6.6743015E-20;    // Gravitational constant km^3 kg^-1 s^-2
        private readonly Double Reg_G = 6.6743000E-11;    // Gravitational constant N m^2 kg^-2 (m s^-2)
        public EphemerisReader(ref BodyList bodyList, ReadSerialized readType)
        {

            if (ReadSerialized.Serialized == readType)
            {
                // Read the serialized ephemeris
                String savedBodyList_Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Properties.Settings.Default.SavedBodyListFile);
                string jsonString = File.ReadAllText(savedBodyList_Path);
                BodyList = JsonSerializer.Deserialize<BodyList>(jsonString)!;
                bodyList = BodyList; // Back to caller
            }
            else
            {
                // Contact JPL and do the progresss dialog
                InitializeComponent();

                progressBar.Value = 0;
                progressBar.MinHeight = 0;
                progressBar.Maximum = (double)bodyList.HowManySelected();
                ShowDialog();
            }

            //BodyList = bodyList;
        }

        public void Start()
        {
            System.ComponentModel.BackgroundWorker worker = new()
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += DoWork;
            worker.ProgressChanged += ProgressChanged;
            worker.RunWorkerCompleted += RunWorkerCompleted;

            worker.RunWorkerAsync(progressBar.Maximum);
        }

        private void ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            Label_BodyName.Content = e.UserState;
        }

        // This event handler deals with the results of the
        // background operation.
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // ********* Serialize and savbe the bodylist
            //String savedBodyList_Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Properties.Settings.Default.SavedBodyListFile);
            //string jsonString = JsonSerializer.Serialize(BodyList);
            //File.WriteAllText(savedBodyList_Path, jsonString);

            this.Close();
        }

        void DoWork(object? sender, DoWorkEventArgs e)
        {

            int[] selected = BodyList.getSelected();
            var bodies = BodyList.Bodies;

            // DT format: 2021-05-03T23:00:00 yyyy-mm-ddThh:mm:ss (round to hour)
            DateTime dt = DateTime.Now;
            DateTime sDT = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            DateTime eDT = sDT.AddHours(2);

            String sDT_Str = sDT.ToString("s");
            String eDT_Str = eDT.ToString("s");

            for (int i = 0; i < Convert.ToInt32(e.Argument); i++) // To number of selected bodies
            {

                Body body = BodyList.Bodies[selected[i]];

                getHorizonsEphemeris(Properties.Settings.Default.HorizonsEphemerisURL, body, sDT_Str, eDT_Str);

                (sender as BackgroundWorker).ReportProgress(1 + i, body.Name);
                Thread.Sleep(100);
            }
        }

        private void getHorizonsEphemeris(String horizonsEphemerisURL, Body body, String sDT_Str, String eDT_Str)
        {

            horizonsEphemerisURL = horizonsEphemerisURL.Replace("{Command}", body.ID)
                                .Replace("{StartTime}", sDT_Str)
                                .Replace("{StopTime}", eDT_Str);

            WebRequest wrGETURL = WebRequest.Create(horizonsEphemerisURL);

            Stream? objStream;
            try
            {
                objStream = wrGETURL.GetResponse().GetResponseStream();
            }
            catch (Exception e) { return; }

            // Write the file (easier for testing)
            //String savedListCSV_Path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Properties.Settings.Default.SavedEphemerisCSVFile);
            //var fileStream = File.Create(savedListCSV_Path);
            //objStream.CopyTo(fileStream);
            //fileStream.Close();

            /*
                  Symbol meaning:

                    0 JDTDB   Julian Day Number
                    1         Calendar Date (TDB) Barycentric Dynamical Time
                    2  X      X-component of position vector (km)
                    3  Y      Y-component of position vector (km)
                    4  Z      Z-component of position vector (km)
                    5  VX     X-component of velocity vector (km/sec)                           
                    6  VY     Y-component of velocity vector (km/sec)                           
                    7  VZ     Z-component of velocity vector (km/sec)                           
                    8  LT     One-way down-leg Newtonian light-time (sec)
                    9  RG     Range; distance from coordinate center (km)
                    10 RR     Range-rate; radial velocity wrt coord. center (km/s

                $$SOE
                data
                $$EOE
            */
            // JDTDB,Calendar Date (TDB),X,Y,Z,VX,VY,VZ,LT,RG,RR,

            StreamReader objReader = new StreamReader(objStream);

            String? sLine;
            String response = new("");
            String? inputLine;

            // Gather the URL response into a String
            sLine = objReader.ReadLine();
            while (sLine != null)
            {
                response = String.Concat(response, sLine + "\n");
                sLine = objReader.ReadLine();
            }

            StringReader stringReader = new(response);

            while ((inputLine = stringReader.ReadLine()) != null)
            {
                if (0 == inputLine.IndexOf("$$SOE")) // If found
                    if ((inputLine = stringReader.ReadLine()) != null)
                    {

                        String[] values = inputLine.Split(",");

                        // Save ephemeris values into body
                        body.X = body.Y = body.Z = body.VX = body.VX = body.VY =
                        body.LT = body.RG = body.RR = -1D;
                        try
                        {
                            body.X = double.Parse(values[2]);
                            body.Y = double.Parse(values[3]);
                            body.Z = double.Parse(values[4]);
                            body.VX = double.Parse(values[5]);
                            body.VY = double.Parse(values[6]);
                            body.VY = double.Parse(values[6]);
                            body.VZ = double.Parse(values[7]);
                            body.LT = double.Parse(values[8]);
                            body.RG = double.Parse(values[9]);
                            body.RR = double.Parse(values[10]);
                        }
                        catch (Exception e) { }

                        break; // From while loop
                    }
            }

            stringReader.Close();

        }

        private void Loaded(object sender, RoutedEventArgs e)
        {
            Start();
        }
    }
}
