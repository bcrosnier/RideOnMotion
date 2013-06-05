using System.Windows;
using RideOnMotion.Inputs.Kinect;
using System.Windows.Controls;
using RideOnMotion.Utilities;
using System.Windows.Input;
using System;
using System.Collections.Specialized;
using System.Windows.Media.Imaging;

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

		public MainWindow()
		{
            InitializeComponent();

            this.mainWindowViewModel = new MainWindowViewModel();
            this.inputController = new KinectSensorController();
            this.DataContext = this.mainWindowViewModel;

            // Bind input menu and fire once
            this.mainWindowViewModel.InputMenuChanged += OnInputMenuChange;
            this.OnInputMenuChange( this, this.mainWindowViewModel.InputMenu );

            ( (INotifyCollectionChanged)this.LogListBox.Items ).CollectionChanged += LogListBox_CollectionChanged;

            /*
            // Drone bitmap placeholder. The image is from Atelier Totori, by the way. --BC
            var uriSource = new Uri( @"http://intech.bcrosnier.com/franck_is_a_lolicon.jpg", UriKind.Absolute );
            
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bi.UriSource = uriSource;
            bi.DownloadCompleted += ( object sender, EventArgs e ) =>
            {
                mainWindowViewModel.DroneImageSource = bi;
            };
            bi.DownloadFailed += ( sender, e ) =>
            {
                RideOnMotion.Logger.Instance.NewEntry( CK.Core.LogLevel.Error, CKTraitTags.Application, "Image download failed: " + e.ErrorException.Message );
            };
            bi.EndInit();
            mainWindowViewModel.DroneImageSource = bi;
            */
		}

        private void LogListBox_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            if ( e.Action == NotifyCollectionChangedAction.Add )
            {
                this.LogListBox.ScrollIntoView( e.NewItems[0] );
            } 
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

		#region KonamiCode
		protected string _konami = string.Empty;
		protected System.Windows.Media.Brush _originalBackground;
		protected UIElement _originalViewBox;
		protected override void OnPreviewKeyUp( KeyEventArgs e )
		{
			this.mainWindowViewModel.OnPreviewKeyUp( e );
		}
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
            this.mainWindowViewModel.OnPreviewKeyDown( e );

            // konami code management.
			if ( _originalBackground == null )
			{
				_originalBackground = MainPanel.Background;
				_originalViewBox = DepthViewerPanel.Children[0];
			}
			string i = "UpUpDownDownLeftRightLeftRightBA";
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
