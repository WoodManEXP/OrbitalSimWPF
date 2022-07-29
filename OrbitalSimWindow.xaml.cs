using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using static OrbitalSimWPF.EphemerisReader;

namespace OrbitalSimWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private BodyList BodyList;
        private SimCamera? SimCamera { get; set; }
        private SimModel? SimModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            stopButton.IsEnabled = false;
            continueButton.IsEnabled = false;

            //String aStr = Properties.Settings.Default.BodiesCSVFile;
            //MessageBox.Show(aStr);

            // Prep the bodies list
            String bodiesCSVFile = Properties.Settings.Default.BodiesCSVFile;
            String savedBodiesCSVFile = Properties.Settings.Default.SimBodiesCSVFile;
            try
            {
                BodyList = new BodyList(bodiesCSVFile, savedBodiesCSVFile);
            }
            catch (Exception ex)
            {
                String aMsg = ex.Message + " " + Properties.Settings.Default.NoBodiesCSVFile + ": " + bodiesCSVFile;
                MessageBox.Show(aMsg, "Oops");
                Environment.Exit(0);
            }
        }

        private void Menu_FileExit(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Bodies(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Button_Bodies");

            bodiesButton.IsEnabled = true;
            continueButton.IsEnabled = false;
            stopButton.IsEnabled = false;
            startButton.IsEnabled = true;

            var dialog = new BodiesListDialog(BodyList);

            bool? result = dialog.ShowDialog();

            PopulateLookAtComboBox();
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            bodiesButton.IsEnabled = false;
            continueButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            // Read the ephemerides from JPL
            new EphemerisReader(ref BodyList, ReadSerialized.Serialized);

            // Instantiate model elements
            SimModel = new();
            SimModel.InitScene(SimViewport, BodyList);

            // Instantiate camera
            Point3D cameraPos = new(6.0E06D, 2.0E06D, 6.0E06D);

            SimCamera = new(SimModel
                          , cameraPos   // Camera's location in the 3D scene
                          , 60D);       // Camera's horizontal field of view in degrees

            SimCamera.LookAt(new(0D, 0D, 0D));

            // Asign the camera to the viewport
            SimViewport.Camera = SimCamera.Camera;

            PopulateLookAtComboBox();
        }

        private void PopulateLookAtComboBox()
        {
            // Populate the LookAt Combobox
            LookAtComboBox.Items.Clear();

            foreach (Body b in BodyList.Bodies)
            {
                if (!b.Selected)
                    continue;

                LookAtComboBox.Items.Add(b.Name);
            }
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            stopButton.IsEnabled = false;
            continueButton.IsEnabled = true;
            bodiesButton.IsEnabled = true;
        }

        private void Button_Continue(object sender, RoutedEventArgs e)
        {
            startButton.IsEnabled = false;
            bodiesButton.IsEnabled = false;
            continueButton.IsEnabled = false;
            stopButton.IsEnabled = true;
        }

        // Create the scene
        // SimViewport is the Viewport3D defined in the xaml file that displays everything
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// e.Delta + is roll forward, - is roll back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimViewportGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            SimCamera.Move((e.Delta > 0) ? SimCamera.MoveDirection.MoveForward : SimCamera.MoveDirection.MoveBackward);
        }

        /// <summary>
        /// Grid containing Viewport3D must have focus before keys flow its way.
        /// Key: Down, Up, LEft, Right
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SimViewportGrid_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            bool look = (e.KeyboardDevice.Modifiers == ModifierKeys.Control | e.KeyboardDevice.Modifiers == ModifierKeys.Alt);

            switch (e.Key)
            {
                case System.Windows.Input.Key.Up:
                    if (look)
                        SimCamera.Look(0D, 1D);
                    else
                        SimCamera.Move(SimCamera.MoveDirection.MoveUp);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Down:
                    if (look)
                        SimCamera.Look(0D, -1D);
                    else
                        SimCamera.Move(SimCamera.MoveDirection.MoveDown);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Left:
                    if (look)
                        SimCamera.Look(1D, 0D);
                    else
                        SimCamera.Move(SimCamera.MoveDirection.MoveLeft);
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Right:
                    if (look)
                        SimCamera.Look(-1D, 0D);
                    else
                        SimCamera.Move(SimCamera.MoveDirection.MoveRight);
                    e.Handled = true;
                    break;
                default:
                    break;
            }

        }

        Boolean ChangingLook = false;
        Point LastMousePt { get; set; }

        private void SimViewportGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            ChangingLook = true;

            LastMousePt = e.GetPosition(this);

            Point p = e.GetPosition(this);

            //System.Diagnostics.Debug.WriteLine("SimViewportGrid_MouseDown " + p.X + " " + p.Y);
        }

        private void SimViewportGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if (!ChangingLook || null == SimCamera)
                return;

            const double minDegrees = 1D, maxDegrees = 5D;

            Point p = e.GetPosition(this);

            double dX = p.X - LastMousePt.X;
            double dY = p.Y - LastMousePt.Y;
            LastMousePt = p;

            SimCamera.Look(-Math.Sign(dX) * Math.Min(Math.Abs(minDegrees * dX), maxDegrees)        // Max +- maxDegrees
                            , -Math.Sign(dY) * Math.Min(Math.Abs(minDegrees * dY), maxDegrees));   // Max +- maxDegrees

            //System.Diagnostics.Debug.WriteLine("SimViewportGrid_MouseMove " + dX + " " + dY);
        }

        private void SimViewportGrid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!ChangingLook)
                return;

            ChangingLook = false;

            Point p = e.GetPosition(this);

            //System.Diagnostics.Debug.WriteLine("SimViewportGrid_MouseUp " + p.X + " " + p.Y);
        }
        
        // https://stackoverflow.com/questions/16966264/what-event-handler-to-use-for-combobox-item-selected-selected-item-not-necessar
        private void LookAtDropDownOpened(object sender, EventArgs e)
        {
            LookAtComboBox.SelectedItem = null;
        }

        private void LookAtSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LookAtComboBox.SelectedItem != null)
                if (null!=SimCamera)
                    SimCamera.LookAt(BodyList.GetPosition((String)LookAtComboBox.SelectedItem));
        }
    }
}
