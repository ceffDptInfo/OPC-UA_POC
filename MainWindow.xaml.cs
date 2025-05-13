using POC.Model;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace POC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected OPCUA OPCUA = new OPCUA();
        public MainWindow()
        {
            InitializeComponent();
            //Start to poll on serv
            this.OPCUA.PollingThreadStart();
            //Listen for a Refresh event
            this.OPCUA.Refresh += (sender, e) =>
            {
                try
                {
                    //Needed to fetch an event from another thread
                    Application.Current?.Dispatcher?.Invoke(() =>
                    {
                        //Update the progress bar
                        ProgressBar.Value = Model.App.ActualPos;
                        //If moving, disable the slider
                        Slider.IsEnabled = Model.App.HasMoveFinished;
                    });
                }
                catch (Exception ex) 
                {
                }
            };

            //Initials values match with the val on serv
            //Split by 10 cause Slider is from 0 to 10
            this.Slider.Value = OPCUA.ReadObject<int>("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.iActualPosition", 4) / 10;
            this.ProgressBar.Value = OPCUA.ReadObject<int>("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.iActualPosition", 4);
        }

        private void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            //All this is for detect when the slider has drag completed
            var slider = sender as Slider;
            var track = slider.Template.FindName("PART_Track", slider) as Track;
            if (track?.Thumb != null)
            {
                track.Thumb.DragCompleted += Thumb_DragCompleted;
            }
        }

        /// <summary>
        /// Send percentage matching to the serv
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            //MessageBox.Show(this.Slider.Value.ToString());
            double percentage = (this.Slider.Value - Slider.Minimum) / (Slider.Maximum - Slider.Minimum) * 100;
            int x = Convert.ToInt32(percentage);
            this.OPCUA.OpcRequestSetTarget(4, x - Model.App.ActualPos);
            this.OPCUA.UpdateBoolToTrue(4);
        }
    }
}