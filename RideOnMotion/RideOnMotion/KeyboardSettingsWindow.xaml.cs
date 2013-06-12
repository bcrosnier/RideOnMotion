using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
// Using settings from these properties
using RideOnMotion.Inputs.Keyboard.Properties;
using System.ComponentModel;
using System.Globalization;

namespace RideOnMotion.UI
{
    /// <summary>
    /// Interaction logic for KeyboardSettingsWindow.xaml
    /// </summary>
    public partial class KeyboardSettingsWindow : Window, INotifyPropertyChanged
    {
        public Key PitchUpKey { get; private set; }
        public Key PitchDownKey { get; private set; }
        public Key RollLeftKey { get; private set; }
        public Key RollRightKey { get; private set; }
        public Key YawLeftKey { get; private set; }
        public Key YawRightKey { get; private set; }
        public Key GazUpKey { get; private set; }
        public Key GazDownKey { get; private set; }
        public Key TakeoffKey { get; private set; }
        public Key LandKey { get; private set; }
        public Key HoverKey { get; private set; }
        public Key CameraSwapKey { get; private set; }
        public Key EmergencyKey { get; private set; }
        public Key FlatTrimKey { get; private set; }
        public Key SpecialActionKey { get; private set; }

        public KeyboardSettingsWindow()
        {
            LoadSettings();
            this.DataContext = this;
            InitializeComponent();
        }

        private void LoadSettings()
        {
            PitchUpKey = (Key)Settings.Default.PitchUp;
            PitchDownKey = (Key)Settings.Default.PitchDown;
            RollLeftKey = (Key)Settings.Default.RollLeft;
            RollRightKey = (Key)Settings.Default.RollRight;
            YawLeftKey = (Key)Settings.Default.YawLeft;
            YawRightKey = (Key)Settings.Default.YawRight;
            GazUpKey = (Key)Settings.Default.GazUp;
            GazDownKey = (Key)Settings.Default.GazDown;
            TakeoffKey = (Key)Settings.Default.TakeOff;
            LandKey = (Key)Settings.Default.Land;
            HoverKey = (Key)Settings.Default.Hover;
            CameraSwapKey = (Key)Settings.Default.CameraSwap;
            EmergencyKey = (Key)Settings.Default.Emergency;
            FlatTrimKey = (Key)Settings.Default.FlatTrim;
            SpecialActionKey = (Key)Settings.Default.SpecialAction;
            RefreshLayoutKeys();
        }

        private void SaveSettings()
        {
            Settings.Default.PitchUp = (int)PitchUpKey;
            Settings.Default.PitchDown = (int)PitchDownKey;
            Settings.Default.RollLeft = (int)RollLeftKey;
            Settings.Default.RollRight = (int)RollRightKey;
            Settings.Default.YawLeft = (int)YawLeftKey;
            Settings.Default.YawRight = (int)YawRightKey;
            Settings.Default.GazUp = (int)GazUpKey;
            Settings.Default.GazDown = (int)GazDownKey;
            Settings.Default.TakeOff = (int)TakeoffKey;
            Settings.Default.Land = (int)LandKey;
            Settings.Default.Hover = (int)HoverKey;
            Settings.Default.CameraSwap = (int)CameraSwapKey;
            Settings.Default.Emergency = (int)EmergencyKey;
            Settings.Default.FlatTrim = (int)FlatTrimKey;
            Settings.Default.SpecialAction = (int)SpecialActionKey;
            Settings.Default.Save();
        }

        private void RestoreDefaultSettings()
        {
            Settings.Default.Reset();
        }

        private void RefreshLayoutKeys() {
            if ( PropertyChanged != null )
            {
                /* Call PropertyChanged with null as PropertyName.
                 * [TODO]
                 * Note: With WPF, this SHOULD refresh all bounds listeners.
                 * However, there is no telling what will happen with other PropertyChanged listeners.
                 * So, yes. This is an ugly hack, and done with full knowledge of it. :)
                 * -- Benjamin
                 * */
                PropertyChanged( this, new PropertyChangedEventArgs( null ) );
            }
        }

        #region INotifyPropertyChanged utilities
        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [notify property change].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnNotifyPropertyChange( string propertyName )
        {
            if ( this.PropertyChanged != null )
            {
                this.PropertyChanged.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Auto-filled with Member name, when called from a property.</param>
        private void RaisePropertyChanged( [System.Runtime.CompilerServices.CallerMemberName] string caller = "" )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }
        #endregion INotifyPropertyChanged utilities

        private void ButtonRestoreDefaults_Click( object sender, RoutedEventArgs e )
        {
            RestoreDefaultSettings();
            LoadSettings();
            RefreshLayoutKeys();
        }

        private void ButtonApply_Click( object sender, RoutedEventArgs e )
        {
            SaveSettings();
            RefreshLayoutKeys();
        }

        private void ButtonCancel_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ButtonOK_Click( object sender, RoutedEventArgs e )
        {
            SaveSettings();
            this.Close();
        }
    }

    public class KeyToStringConverter : IValueConverter
    {
        public object Convert( object value, Type targetType,
            object parameter, CultureInfo culture )
        {
            return ( (Key)value ).ToString();
        }

        public object ConvertBack( object value, Type targetType,
            object parameter, CultureInfo culture )
        {
            Key k;
            if ( Enum.TryParse<Key>( (String)value, out k ) )
            {
                return k;
            }
            else
            {
                return null;
            }

        }
    }
}
