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

        private void LogString_TextChanged( object sender, System.Windows.Controls.TextChangedEventArgs e )
        {
            TextBox tb = (TextBox)sender;

            //tb.SelectionStart = tb.Text.Length;
            tb.ScrollToEnd();
        }


	}
}
