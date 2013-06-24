using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RideOnMotion.UI
{
    /// <summary>
    /// A progress bar, looking like a simple ruler.
    /// Made only of rectangles.
    /// </summary>
    public class RulerProgressBar : RangeBase
    {
        /// <summary>
        /// Color of the progress bar.
        /// </summary>
        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register( "Fill", typeof( Brush ), typeof( RulerProgressBar ), new UIPropertyMetadata( Brushes.Orange ) );

        /// <summary>
        /// Thickness of the lines, in pixels.
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register( "Thickness", typeof( Double ), typeof( RulerProgressBar ), new UIPropertyMetadata( (Double)2.0 ) );

        /// <summary>
        /// Number of bars to show, excluding the first and the last.
        /// </summary>
        public static readonly DependencyProperty BarNumberProperty =
            DependencyProperty.Register( "BarNumber", typeof( Int32 ), typeof( RulerProgressBar ), new UIPropertyMetadata( (Int32)9 ) );

        /// <summary>
        /// Color of the progress bar.
        /// </summary>
        public Brush Fill
        {
            get { return base.GetValue( FillProperty ) as Brush; }
            set { base.SetValue( FillProperty, value ); }
        }

        /// <summary>
        /// Thickness of the lines, in pixels.
        /// </summary>
        public Double Thickness
        {
            get { return (Double)base.GetValue( ThicknessProperty ); }
            set { base.SetValue( ThicknessProperty, value ); }
        }

        /// <summary>
        /// Number of bars to show, excluding the first and the last.
        /// </summary>
        public Int32 BarNumber
        {
            get { return (Int32)base.GetValue( BarNumberProperty ); }
            set { base.SetValue( BarNumberProperty, value ); }
        }

        /// <summary>
        /// A progress bar, looking like a simple ruler.
        /// Made only of rectangles.
        /// </summary>
        static RulerProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( RulerProgressBar ), new FrameworkPropertyMetadata( typeof( RulerProgressBar ) ) );
        }

        /// <summary>
        /// A progress bar, looking like a simple ruler.
        /// Made only of rectangles.
        /// </summary>
        public RulerProgressBar()
        {
            this.ValueChanged += RangeProgressBar_ValueChanged;
        }

        /// <summary>
        /// Fired by RangeBase when Value changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RangeProgressBar_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
        {
            // Force a new render.
            this.InvalidateVisual();
        }

        /// <summary>
        /// Fired by WPF whenever it wants to display it.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender( DrawingContext drawingContext )
        {
            double shapeWidth = this.ActualWidth * ( ( Value - Minimum ) / ( Maximum - Minimum ) );
            double barSpacing = ActualWidth / ( BarNumber + 1 );

            Rect baseRect = new Rect( 0, this.ActualHeight - Thickness, shapeWidth, Thickness );
            drawingContext.DrawRectangle( Fill, null, baseRect );

            if ( Value > Minimum )
            {
                // First bar
                drawingContext.DrawRectangle( Fill, null, new Rect( 0, 0, Thickness, ActualHeight ) );
            }

            if ( Value >= Maximum )
            {
                // Last bar
                drawingContext.DrawRectangle( Fill, null, new Rect( ActualWidth - Thickness, 0, Thickness, ActualHeight ) );
            }

            for ( double pos = barSpacing; pos <= shapeWidth && pos < ActualWidth; pos += barSpacing )
            {
                // Every other bar
                var x = pos - ( Thickness / 2 );
                var y = ActualHeight - (ActualHeight / 3);
                drawingContext.DrawRectangle( Fill, null, new Rect( x, y, Thickness, ActualHeight / 3 ) );
            }

            // Call method of base class.
            base.OnRender( drawingContext );
        }

    }
}
