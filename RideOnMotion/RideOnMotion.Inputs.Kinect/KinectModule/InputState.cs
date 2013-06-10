using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Inputs.Kinect
{
	public class InputState
	{
		private static int _controlNumber = 8;
		private bool[] _currentInputState = new bool[_controlNumber];

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
			if ( inputs.Length == 8 )
			{
				bool inputChanged = false;
				if ( !Enumerable.SequenceEqual( inputs, CurrentInputState ) )
				{
					for ( int i = 0; i < 8; i++ )
					{
						ChangeInputState( i, inputs[i] );
					}
					inputChanged = true;
				}
				return inputChanged;
			}
			else
			{
				Logger.Instance.NewEntry( CKLogLevel.Fatal, CKTraitTags.Kinect, "Array out of bound in commands attribution" );
				return false;
			}
		}

		public InputState()
		{
		}
	}
}
