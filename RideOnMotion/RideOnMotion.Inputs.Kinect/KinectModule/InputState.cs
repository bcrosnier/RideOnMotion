using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace RideOnMotion.Inputs.Kinect
{
	public class InputState
	{
		private static int _controlNumber = 2;
		private bool[] _currentInputState = new bool[_controlNumber];
        private IActivityLogger _logger;

		public bool[] CurrentInputState
		{
			get
			{
				return _currentInputState;
			}
		}

		private void ChangeInputState( int index, bool newValue )
		{
			_currentInputState[index] = newValue;
		}

		/// <summary>
		/// Check the Given input and change it if needed
		/// </summary>
		/// <param name="inputs">The input to check</param>
		/// <returns>true if the operation changed something</returns>
		public bool CheckInput( bool[] inputs )
		{
			if ( inputs.Length == 2 )
			{
				bool inputChanged = false;
				if ( !Enumerable.SequenceEqual( inputs, CurrentInputState ) )
				{
					for ( int i = 0; i < 2; i++ )
					{
						ChangeInputState( i, inputs[i] );
					}
					inputChanged = true;
				}
				return inputChanged;
			}
			else
			{
                _logger.Error( "Array out of bound in commands attribution" );
				return false;
			}
		}

		public InputState( IActivityLogger parentLogger )
        {
            _logger = new DefaultActivityLogger();
            _logger.AutoTags = ActivityLogger.RegisteredTags.FindOrCreate( "InputState" );
            _logger.Output.BridgeTo( parentLogger );
		}
	}
}
