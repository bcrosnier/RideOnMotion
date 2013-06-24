using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RideOnMotion.UI
{
    /// <summary>
    /// A user control used to display four bars on the outsides of an empty center area.
    /// </summary>
    public partial class DirectionDisplayControl : UserControl
    {
        private enum Directions
        {
            Right,
            Down,
            Left,
            Up
        }

        /// <summary>
        /// Control to display as the left bar.
        /// </summary>
        public static readonly DependencyProperty LeftRangeProperty =
            DependencyProperty.Register( "LeftRange", typeof( RangeBase ), typeof( DirectionDisplayControl ) );

        /// <summary>
        /// Control to display as the top bar.
        /// </summary>
        public static readonly DependencyProperty UpRangeProperty =
            DependencyProperty.Register( "UpRange", typeof( RangeBase ), typeof( DirectionDisplayControl ) );

        /// <summary>
        /// Control to display as the right bar.
        /// </summary>
        public static readonly DependencyProperty RightRangeProperty =
            DependencyProperty.Register( "RightRange", typeof( RangeBase ), typeof( DirectionDisplayControl ) );

        /// <summary>
        /// Control to display as the bottom bar.
        /// </summary>
        public static readonly DependencyProperty DownRangeProperty =
            DependencyProperty.Register( "DownRange", typeof( RangeBase ), typeof( DirectionDisplayControl ) );

        /// <summary>
        /// Width of the center area. Also controls the width/height of the bars.
        /// </summary>
        public static readonly DependencyProperty CenterWidthProperty =
            DependencyProperty.Register( "CenterWidth", typeof( double ), typeof( DirectionDisplayControl ), new PropertyMetadata( (double)50 ) );

        /// <summary>
        /// Height of the center area. Also controls the width/height of the bars.
        /// </summary>
        public static readonly DependencyProperty CenterHeightProperty =
            DependencyProperty.Register( "CenterHeight", typeof( double ), typeof( DirectionDisplayControl ), new PropertyMetadata( (double)50 ) );

        private Transform _rotate90Transform;
        private Transform _horizontalFlipTransform;
        private Transform _verticalFlipTransform;
        private TransformGroup _upRangeTransformGroup;
        private Dictionary<Directions, RangeBase> _ranges;

        double _horizontalControlWidth;
        double _horizontalControlHeight;
        double _verticalControlWidth;
        double _verticalControlHeight;

        /// <summary>
        /// Control to display as the left bar.
        /// </summary>
        public RangeBase LeftRange
        {
            get { return base.GetValue( LeftRangeProperty ) as RangeBase; }
            set { base.SetValue( LeftRangeProperty, value ); PrepareLeftRange(); }
        }
        /// <summary>
        /// Control to display as the top bar.
        /// </summary>
        public RangeBase UpRange
        {
            get { return base.GetValue( UpRangeProperty ) as RangeBase; }
            set { base.SetValue( UpRangeProperty, value ); PrepareUpRange(); }
        }
        /// <summary>
        /// Control to display as the right bar.
        /// </summary>
        public RangeBase RightRange
        {
            get { return base.GetValue( RightRangeProperty ) as RangeBase; }
            set { base.SetValue( RightRangeProperty, value ); PrepareRightRange(); }
        }
        /// <summary>
        /// Control to display as the bottom bar.
        /// </summary>
        public RangeBase DownRange
        {
            get { return base.GetValue( DownRangeProperty ) as RangeBase; }
            set { base.SetValue( DownRangeProperty, value ); PrepareDownRange(); }
        }
        /// <summary>
        /// Width of the center area. Also controls the width/height of the bars.
        /// </summary>
        public double CenterWidth
        {
            get { return (double)base.GetValue( CenterWidthProperty ); }
            set { base.SetValue( CenterWidthProperty, value ); RecalculateSizes(); }
        }
        /// <summary>
        /// Height of the center area. Also controls the width/height of the bars.
        /// </summary>
        public double CenterHeight
        {
            get { return (double)base.GetValue( CenterHeightProperty ); }
            set { base.SetValue( CenterHeightProperty, value ); RecalculateSizes(); }
        }

        /// <summary>
        /// A user control used to display four bars on the outsides of an empty center area.
        /// </summary>
        public DirectionDisplayControl()
        {
            _rotate90Transform = new RotateTransform( 90 );
            _horizontalFlipTransform = new ScaleTransform( -1, 1 );
            _verticalFlipTransform = new ScaleTransform( 1, -1 );

            _upRangeTransformGroup = new TransformGroup();
            _upRangeTransformGroup.Children.Add( _rotate90Transform );
            _upRangeTransformGroup.Children.Add( _verticalFlipTransform );

            _ranges = new Dictionary<Directions, RangeBase>( 4 );
            _ranges.Add( Directions.Up, null );
            _ranges.Add( Directions.Down, null );
            _ranges.Add( Directions.Left, null );
            _ranges.Add( Directions.Right, null );

            this.SizeChanged += OnSizeChanged;
            this.Loaded += DirectionDisplayControl_Loaded;
            InitializeComponent();
        }

        /// <summary>
        /// Fired when display area size changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if ( e.NewSize.Height < 100 || e.NewSize.Width < 100 )
                return;

            RecalculateSizes();
        }

        /// <summary>
        /// Prepare left bar.
        /// </summary>
        void PrepareLeftRange()
        {
            if ( _ranges[Directions.Left] != null )
            {
                _canvas.Children.Remove( _ranges[Directions.Left] );
            }
            LeftRange.Width = _horizontalControlWidth;
            LeftRange.Height = _horizontalControlHeight;
            LeftRange.RenderTransform = _horizontalFlipTransform;
            _canvas.Children.Add( LeftRange );
            Canvas.SetTop( LeftRange, _verticalControlWidth );
            Canvas.SetLeft( LeftRange, _horizontalControlWidth );
            _ranges[Directions.Left] = LeftRange;
        }

        /// <summary>
        /// Prepare right bar.
        /// </summary>
        void PrepareRightRange()
        {
            if ( _ranges[Directions.Right] != null )
            {
                _canvas.Children.Remove( _ranges[Directions.Right] );
            }
            RightRange.Width = _horizontalControlWidth;
            RightRange.Height = _horizontalControlHeight;
            _canvas.Children.Add( RightRange );
            Canvas.SetTop( RightRange, _verticalControlWidth );
            Canvas.SetLeft( RightRange, _horizontalControlWidth + CenterWidth );
            _ranges[Directions.Right] = RightRange;
        }

        /// <summary>
        /// Prepare bottom bar.
        /// </summary>
        void PrepareDownRange()
        {
            if ( _ranges[Directions.Down] != null )
            {
                _canvas.Children.Remove( _ranges[Directions.Down] );
            }
            DownRange.Width = _verticalControlWidth;
            DownRange.Height = _verticalControlHeight;
            DownRange.RenderTransform = _rotate90Transform;
            _canvas.Children.Add( DownRange );
            Canvas.SetTop( DownRange, _verticalControlWidth + CenterHeight );
            Canvas.SetLeft( DownRange, _horizontalControlWidth + CenterWidth );
            _ranges[Directions.Down] = DownRange;
        }

        /// <summary>
        /// Prepare top bar.
        /// </summary>
        void PrepareUpRange()
        {
            if ( _ranges[Directions.Up] != null )
            {
                _canvas.Children.Remove( _ranges[Directions.Up] );
            }
            UpRange.Width = _verticalControlWidth;
            UpRange.Height = _verticalControlHeight;
            UpRange.RenderTransform = _upRangeTransformGroup;
            _canvas.Children.Add( UpRange );
            Canvas.SetTop( UpRange, _verticalControlWidth );
            Canvas.SetLeft( UpRange, _horizontalControlWidth + CenterWidth );
            _ranges[Directions.Up] = UpRange;
        }

        void DirectionDisplayControl_Loaded( object sender, RoutedEventArgs e )
        {
            Prepare();
        }

        /// <summary>
        /// Force a recalculation of the area size and bars height/widths, then re-display them.
        /// </summary>
        void RecalculateSizes()
        {
            _horizontalControlWidth = ( ActualWidth - CenterWidth ) / 2;
            _horizontalControlHeight = CenterHeight;
            _verticalControlWidth = ( ActualHeight - CenterHeight ) / 2;
            _verticalControlHeight = CenterWidth;
            Prepare();
        }

        /// <summary>
        /// Reset and display all bars.
        /// </summary>
        public void Prepare()
        {
            if ( LeftRange != null )
                PrepareLeftRange();
            if ( RightRange != null )
                PrepareRightRange();
            if ( UpRange != null )
                PrepareUpRange();
            if ( DownRange != null )
                PrepareDownRange();
        }
    }
}
