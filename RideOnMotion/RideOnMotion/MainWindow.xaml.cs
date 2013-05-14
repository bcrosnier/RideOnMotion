using System.Windows;
using RideOnMotion.Inputs.Kinect;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Collections.Specialized;

namespace RideOnMotion.UI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        /// <summary>
        /// Model view for this window.
        /// </summary>
        private MainWindowViewModel mainWindowViewModel;

        private MenuItem _activeInputMenuItem;

		public MainWindow()
		{
            InitializeComponent();

            this.mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = this.mainWindowViewModel;

            // Bind input menu and fire once
            this.mainWindowViewModel.InputMenuChanged += OnInputMenuChange;
            this.OnInputMenuChange( this, this.mainWindowViewModel.InputMenu );

            ( (INotifyCollectionChanged)this.LogListBox.Items ).CollectionChanged += LogListBox_CollectionChanged;
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.mainWindowViewModel.Stop(); 
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
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
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
