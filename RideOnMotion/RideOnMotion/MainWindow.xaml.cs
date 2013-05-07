using System.Windows;
using RideOnMotion.Inputs.Kinect;
using System.Windows.Controls;

namespace RideOnMotion.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/// <summary>
		/// Active Kinect sensor controller.
        /// </summary>
        private IDroneInputController inputController;

        /// <summary>
        /// Model view for this window.
        /// </summary>
        private MainWindowViewModel mainWindowViewModel;


		public MainWindow()
		{
            InitializeComponent();

            this.inputController = new KinectSensorController();
            this.mainWindowViewModel = new MainWindowViewModel( inputController );
            this.DataContext = this.mainWindowViewModel;

            // Bind input menu
            if ( this.inputController.InputMenu != null )
            {
                this.MenuBar.Items.Add( this.inputController.InputMenu );
            }
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
            prepareInput();
		}

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.inputController.Stop(); 
        }

        private void prepareInput()
        {
            if ( this.inputController.InputStatus == DroneInputStatus.Disconnected )
            {
                MessageBox.Show( "No input device detected.\nPlease ensure it is plugged in and correctly installed.", "No input detected" );
            }
            else
            {
                // Start Kinect
                this.inputController.Start(); // Blocking.
            }
        }

        /// <summary>
        /// File -> Quit action
        /// </summary>
        private void MenuItem_Quit_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void LogString_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
        {
            TextBox tb = (TextBox)sender;

            //tb.SelectionStart = tb.Text.Length;
            tb.ScrollToEnd();
        }


	}
}
