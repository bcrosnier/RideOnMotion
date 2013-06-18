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
    /// Trigger area, representing a space to move a hand in.
    /// Centered in a given area.
    /// </summary>
    internal class TriggerArea
    {
        public enum Buttons { Center = 0 };

        public int AreaWidth { get; private set; }
        public int AreaHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int DeadZoneWidth { get; private set; }
        public int DeadZoneHeight { get; private set; }

        public ICaptionArea CenterButton { get; private set; }

        public Dictionary<Buttons, ICaptionArea> DeadZoneCaptionsCollection { get; private set; }

		//private Action Quack;

        /// <summary>
        /// Create a trigger area, containing a triggerable dead zone.
        /// </summary>
        /// <param name="areaWidth">Total width of the area to use ; dead zone will be centered in it.</param>
        /// <param name="areaHeight">Total height of the area to use ; dead zone will be centered in it.</param>
        /// <param name="offsetX">X offset to the area</param>
        /// <param name="offsetY">Y offset to the area</param>
        /// <param name="deadZoneWidth">Width of each button. Side size of the area.</param>
        /// <param name="deadZoneHeight">Height of each button; "thickness" of the area.</param>
		/// <param name="isLeftArea">True if the trigger area is on the left, false if it is on the right</param>
        public TriggerArea( int areaWidth, int areaHeight, int offsetX, int offsetY, int deadZoneWidth, int deadZoneHeight, bool isLeftArea )
        {
            this.AreaHeight = areaHeight;
            this.AreaWidth = areaWidth;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.DeadZoneWidth = deadZoneWidth;
            this.DeadZoneHeight = deadZoneHeight;

            this.DeadZoneCaptionsCollection = new Dictionary<Buttons, ICaptionArea>();

			//Quack = new Action( () => {
			//	System.Media.SoundPlayer sp = new System.Media.SoundPlayer( RideOnMotion.Inputs.Kinect.Properties.Resources.quack );
			//	sp.Play();
			//} );

            generateButtonCaptions(isLeftArea);
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="isLeftArea">Specify if the buttons are on the left (true) or on the right (false)</param>
        public void generateButtonCaptions( bool isLeftArea )
        {
            int center = 0;

			if ( isLeftArea )
			{
				center = 1;
			}
			else if ( !isLeftArea )
			{
				center = 2;
			}
            int horizontalMargin = AreaWidth / 2 - DeadZoneWidth / 2;
            int verticalMargin = AreaHeight / 2 - DeadZoneHeight / 2;

            ICaptionArea centerCaption = createCaptionArea(
                "Center button",
                OffsetX + horizontalMargin,
                OffsetY + verticalMargin,
                DeadZoneWidth,
                DeadZoneHeight,
				center
            );

            DeadZoneCaptionsCollection.Add( Buttons.Center, centerCaption );

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
