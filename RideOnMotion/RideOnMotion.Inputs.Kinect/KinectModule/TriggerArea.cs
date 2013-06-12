using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Trigger zone, reprensenting 4 overlayed TriggerButtons as sides of a square.
    /// Centered in a given area.
    /// </summary>
    internal class TriggerArea
    {
        public enum Buttons { Center = 0 };

        public int AreaWidth { get; private set; }
        public int AreaHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int ButtonWidth { get; private set; }
        public int ButtonHeight { get; private set; }

        public ICaptionArea CenterButton { get { return this.TriggerCaptionsCollection[Buttons.Center]; } }

        public Dictionary<Buttons, ICaptionArea> TriggerCaptionsCollection { get; private set; }

		//private Action Quack;

        /// <summary>
        /// Create a trigger area, with 4 triggerable captions.
        /// </summary>
        /// <param name="areaWidth">Total width of the zone to use ; area will be centered in it.</param>
        /// <param name="areaHeight">Total height of the zone to use ; area will be centered in it.</param>
        /// <param name="offsetX">X offset to all buttons</param>
        /// <param name="offsetY">Y offset to all buttons</param>
        /// <param name="buttonWidth">Width of each button. Side size of the area.</param>
        /// <param name="buttonHeight">Height of each button; "thickness" of the area.</param>
		/// <param name="position">True if the trigger area is on the left, false if it is on the right</param>
        public TriggerArea( int areaWidth, int areaHeight, int offsetX, int offsetY, int buttonWidth, int buttonHeight, bool position )
        {
            this.AreaHeight = areaHeight;
            this.AreaWidth = areaWidth;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.ButtonWidth = buttonWidth;
            this.ButtonHeight = buttonHeight;

            this.TriggerCaptionsCollection = new Dictionary<Buttons, ICaptionArea>();

			//Quack = new Action( () => {
			//	System.Media.SoundPlayer sp = new System.Media.SoundPlayer( RideOnMotion.Inputs.Kinect.Properties.Resources.quack );
			//	sp.Play();
			//} );

            generateButtonCaptions(position);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="position">Specify if the buttons are on the left (true) or on the right (false)</param>
        public void generateButtonCaptions( bool position)
        {
            int center = 0;

			if ( position )
			{
				center = 1;
			}
			else if ( !position )
			{
				center = 2;
			}
            int horizontalMargin = ( AreaWidth - ButtonWidth ) / 2;
            int verticalMargin = ( AreaHeight - ButtonWidth ) / 2;

            ICaptionArea centerCaption = createCaptionArea(
                "Center button",
                OffsetX + horizontalMargin,
                OffsetY + verticalMargin,
                ButtonWidth,
                ButtonHeight,
				center
            );

            TriggerCaptionsCollection.Add( Buttons.Center, centerCaption );

            // Fire stuff to update rectangles here. --BC
        }

        private ICaptionArea createCaptionArea( string name, int depthX, int depthY, int width, int height , int id)
        {
            //DepthImagePoint depthPoint = new DepthImagePoint() { X = depthX, Y = depthY };
            //SkeletonPoint skelPoint = _pointConverter( depthPoint );
			// BIIIIIATCH !!!!
            return new CaptionArea(
                    name,
                    new Point(
                        depthX,
                        depthY
                    ),
                    width,
                    height,
					id
               );
        }
    }
}
