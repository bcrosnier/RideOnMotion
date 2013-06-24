using RideOnMotion.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace RideOnMotion.UI
{
    class InputProgressBarSet
    {
        public RangeBase PositivePitchProgressBar
        {
            get;
            private set;
        }
        public RangeBase NegativePitchProgressBar
        {
            get;
            private set;
        }
        public RangeBase PositiveRollProgressBar
        {
            get;
            private set;
        }
        public RangeBase NegativeRollProgressBar
        {
            get;
            private set;
        }


        public RangeBase PositiveGazProgressBar
        {
            get;
            private set;
        }
        public RangeBase NegativeGazProgressBar
        {
            get;
            private set;
        }
        public RangeBase PositiveYawProgressBar
        {
            get;
            private set;
        }
        public RangeBase NegativeYawProgressBar
        {
            get;
            private set;
        }

        public InputProgressBarSet( double thickness, Brush color )
        {
            this.PositivePitchProgressBar = new ProgressBar();
            this.NegativePitchProgressBar = new ProgressBar();
            this.PositiveRollProgressBar = new ProgressBar();
            this.NegativeRollProgressBar = new ProgressBar();

            this.PositiveGazProgressBar = new ProgressBar();
            this.NegativeGazProgressBar = new ProgressBar();
            this.PositiveYawProgressBar = new ProgressBar();
            this.NegativeYawProgressBar = new ProgressBar();

            this.PositivePitchProgressBar.Minimum = 0.0;
            this.NegativePitchProgressBar.Minimum = 0.0;
            this.PositiveRollProgressBar.Minimum = 0.0;
            this.NegativeRollProgressBar.Minimum = 0.0;
            this.PositiveGazProgressBar.Minimum = 0.0;
            this.NegativeGazProgressBar.Minimum = 0.0;
            this.PositiveYawProgressBar.Minimum = 0.0;
            this.NegativeYawProgressBar.Minimum = 0.0;

            this.PositivePitchProgressBar.Maximum = 1.0;
            this.NegativePitchProgressBar.Maximum = 1.0;
            this.PositiveRollProgressBar.Maximum = 1.0;
            this.NegativeRollProgressBar.Maximum = 1.0;
            this.PositiveGazProgressBar.Maximum = 1.0;
            this.NegativeGazProgressBar.Maximum = 1.0;
            this.PositiveYawProgressBar.Maximum = 1.0;
            this.NegativeYawProgressBar.Maximum = 1.0;

            ResetState();
        }

        public void UpdateInputState( InputState newInputState )
        {
            if ( newInputState == null )
            {
                ResetState();
            }
            else
            {
                this.PositivePitchProgressBar.Value = newInputState.Pitch;
                this.NegativePitchProgressBar.Value = -newInputState.Pitch;
                this.NegativeRollProgressBar.Value = -newInputState.Roll;
                this.PositiveRollProgressBar.Value = newInputState.Roll;

                this.PositiveGazProgressBar.Value = newInputState.Gaz;
                this.NegativeGazProgressBar.Value = -newInputState.Gaz;
                this.NegativeYawProgressBar.Value = -newInputState.Yaw;
                this.PositiveYawProgressBar.Value = newInputState.Yaw;
            }
        }

        public void ResetState()
        {
            this.PositivePitchProgressBar.Value = 0.0;
            this.NegativePitchProgressBar.Value = 0.0;
            this.PositiveRollProgressBar.Value = 0.0;
            this.NegativeRollProgressBar.Value = 0.0;

            this.PositiveGazProgressBar.Value = 0.0;
            this.NegativeGazProgressBar.Value = 0.0;
            this.NegativeYawProgressBar.Value = 0.0;
            this.PositiveYawProgressBar.Value = 0.0;
        }
    }
}
