using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace RideOnMotion.Inputs.Kinect
{
    /// <summary>
    /// Trigger zone, reprensenting 4 overlayed TriggerButtons as sides of a square.
    /// Centered in a given area.
    /// </summary>
    internal class TriggerArea
    {
        public enum Buttons { Up = 0, Down = 1, Left = 2, Right = 3 };

        public int AreaWidth { get; private set; }
        public int AreaHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }
        public int ButtonWidth { get; private set; }
        public int ButtonHeight { get; private set; }

        public ICaptionArea LeftButton { get { return this.TriggerCaptionsCollection[Buttons.Left]; } }
        public ICaptionArea RightButton { get { return this.TriggerCaptionsCollection[Buttons.Right]; } }
        public ICaptionArea UpButton { get { return this.TriggerCaptionsCollection[Buttons.Up]; } }
        public ICaptionArea DownButton { get { return this.TriggerCaptionsCollection[Buttons.Down]; } }

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
			int up = 0;
			int down = 0;
			int left = 0;
			int right = 0;

			if ( position )
			{
				up = 1;
				down = 2;
				left = 4;
				right = 8;
			}
			else if ( !position )
			{
				up = 16;
				down = 32;
				left = 64;
				right = 128;
			}
            int horizontalMargin = ( AreaWidth - ButtonWidth ) / 2;
            int verticalMargin = ( AreaHeight - ButtonWidth ) / 2;

            ICaptionArea upCaption = createCaptionArea(
                "Up button",
                OffsetX + horizontalMargin,
                OffsetY + verticalMargin,
                ButtonWidth,
                ButtonHeight,
				up
            );

            ICaptionArea leftCaption = createCaptionArea(
                "Left button",
                OffsetX + horizontalMargin,
                OffsetY + verticalMargin,
                ButtonHeight,
                ButtonWidth,
				left
            );

            ICaptionArea downCaption = createCaptionArea(
                "Down button",
                OffsetX + horizontalMargin,
                OffsetY + verticalMargin + ButtonWidth - ButtonHeight,
                ButtonWidth,
                ButtonHeight,
				down
            );

            ICaptionArea rightCaption = createCaptionArea(
                "Right button",
                OffsetX + horizontalMargin + ButtonWidth - ButtonHeight,
                OffsetY + verticalMargin,
                ButtonHeight,
                ButtonWidth,
				right
            );

			//((CaptionArea)upCaption).AddFunction( Quack );
			//((CaptionArea)downCaption).AddFunction( Quack );
			//((CaptionArea)leftCaption).AddFunction( Quack );
			//((CaptionArea)rightCaption).AddFunction( Quack );

            TriggerCaptionsCollection.Add( Buttons.Up, upCaption );
            TriggerCaptionsCollection.Add( Buttons.Down, downCaption );
            TriggerCaptionsCollection.Add( Buttons.Left, leftCaption );
            TriggerCaptionsCollection.Add( Buttons.Right, rightCaption );

            // Fire stuff to update rectangles here. --BC
        }

        private ICaptionArea createCaptionArea( string name, int depthX, int depthY, int width, int height , int id)
        {
            //DepthImagePoint depthPoint = new DepthImagePoint() { X = depthX, Y = depthY };
            //SkeletonPoint skelPoint = _pointConverter( depthPoint );
            return new CaptionArea(
                    name,
                    new Inputs.Kinect.Point(
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
