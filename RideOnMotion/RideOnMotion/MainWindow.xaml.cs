using System.Windows;
using RideOnMotion.Inputs.Kinect;
using System.Windows.Controls;
using RideOnMotion.Utilities;
using System.Windows.Input;
using System;

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

        private MenuItem _activeInputMenuItem;

		DroneInitializer drone;

		public MainWindow()
		{
            InitializeComponent();

            this.mainWindowViewModel = new MainWindowViewModel();
            this.inputController = new KinectSensorController();
            this.DataContext = this.mainWindowViewModel;

            // Bind input menu and fire once
            this.mainWindowViewModel.InputMenuChanged += OnInputMenuChange;
            this.OnInputMenuChange( this, this.mainWindowViewModel.InputMenu );
            // Bind input menu
            if ( this.inputController.InputMenu != null )
            {
                this.MenuBar.Items.Add( this.inputController.InputMenu );
            }

			drone = new DroneInitializer();
		}

        private void OnInputMenuChange( object sender, MenuItem e )
        {
            if ( this._activeInputMenuItem != null )
            {
                this.MenuBar.Items.Remove( this._activeInputMenuItem );
            }

            if ( e != null )
            {
                this.MenuBar.Items.Add( e );
            }

            this._activeInputMenuItem = e;
        }

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
		}

		private void MainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
		{
			this.mainWindowViewModel.Stop();
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

		#region KonamiCode
		protected string _konami = string.Empty;
		protected System.Windows.Media.Brush _originalBackground;
		protected UIElement _originalViewBox;
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
			if( e.Key.ToString() == "Space")
			{
				drone.DroneCommand.Takeoff();
			}

			if( e.Key.ToString() == "A" )
			{
				drone.DroneCommand.Navigate(-1, 1, 0, 0);
			}
			if( e.Key.ToString() == "Z" )
			{
				drone.DroneCommand.Navigate( 0, 1, 0, 0 );
			}
			if( e.Key.ToString() == "E" )
			{
				drone.DroneCommand.Navigate( 1, 1, 0, 0 );
			}
			if( e.Key.ToString() == "Q" )
			{
				drone.DroneCommand.Navigate( -1, 0, 0, 0 );
			}
			if( e.Key.ToString() == "S" )
			{
				drone.DroneCommand.Navigate( 0, 0, 0, 0 );
			}
			if( e.Key.ToString() == "D" )
			{
				drone.DroneCommand.Navigate( 1, 0, 0, 0 );
			}
			if( e.Key.ToString() == "W" )
			{
				drone.DroneCommand.Navigate( -1, -1, 0, 0 );
			}
			if( e.Key.ToString() == "X" )
			{
				drone.DroneCommand.Navigate( 0, -1, 0, 0 );
			}
			if( e.Key.ToString() == "C" )
			{
				drone.DroneCommand.Navigate( 1, -1, 0, 0 );
			}

			if( e.Key.ToString() == "Enter" )
			{
				drone.DroneCommand.Takeoff();
			}

			if ( _originalBackground == null )
			{
				_originalBackground = MainPanel.Background;
				_originalViewBox = DepthViewerPanel.Children[0];
			}
			string i = "UpUpDownDownLeftRightLeftRightBAReturn";
			if ( e.Key.ToString() == "Up" && _konami != "Up" )
			{
				_konami = "";
			}
			_konami = ( _konami + e.Key.ToString() );
			// Debug.Print(konami)
			if ( ( _konami == i ) )
			{
				mainWindowViewModel.Konami = true;
				string fileName = "..\\..\\Resources\\mad_duck.jpg";
				System.Windows.Media.Imaging.BitmapImage image = new System.Windows.Media.Imaging.BitmapImage( new System.Uri( fileName, System.UriKind.Relative ) );
				System.Windows.Media.ImageBrush brush = new System.Windows.Media.ImageBrush();
				brush.ImageSource = image;
				MainPanel.Background = brush;


				Viewbox v = new Viewbox();
				MediaElement me = new MediaElement();
				fileName = "..\\..\\Resources\\Star Wars Ducks.mp4";
				me.Source = new System.Uri( fileName, System.UriKind.Relative );
				me.LoadedBehavior = MediaState.Manual;
				me.MediaEnded += new RoutedEventHandler( delegate( object s, RoutedEventArgs re ) { me.Stop(); me.Play(); } );
				me.Play();
				v.Child = me;
				DepthViewerPanel.Children.RemoveAt( 0 );
				DepthViewerPanel.Children.Add( v );
			}
			else if ( _konami.Length > i.Length )
			{
				mainWindowViewModel.Konami = false;
				MainPanel.Background = _originalBackground;
				DepthViewerPanel.Children.RemoveAt( 0 );
				DepthViewerPanel.Children.Add( _originalViewBox );
			}
		}

		#endregion //KonamiCode

	}
}
