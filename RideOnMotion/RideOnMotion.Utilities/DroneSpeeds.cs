using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{
	public class DroneSpeeds
	{
		float _droneTranslationSpeed;
		float _droneRotationSpeed;
		float _droneElevationSpeed;

		/// <summary>
		/// Set drone speeds
		/// </summary>
		/// <param name="TranslationSpeed">Translation</param>
		/// <param name="RotationSpeed">Rotation</param>
		/// <param name="ElevationSpeed">Elevation</param>
		public DroneSpeeds( float TranslationSpeed, float RotationSpeed, float ElevationSpeed )
		{

			if ( TranslationSpeed > 0.0 && TranslationSpeed <= 1.0 )
			{
				this._droneTranslationSpeed = TranslationSpeed;
			}

			if ( RotationSpeed > 0.0 && RotationSpeed <= 1.0 )
			{
				this._droneRotationSpeed = RotationSpeed;
			}

			if ( ElevationSpeed > 0.0 && ElevationSpeed <= 1.0 )
			{
				this._droneElevationSpeed = ElevationSpeed;
			}
		}

		public float DroneElevationSpeed
		{
			get
			{
				return _droneElevationSpeed;
			}
			set
			{
				if ( value > 0 && value <= 1 )
				{
					_droneElevationSpeed = value;
				}
			}
		}
		public float DroneRotationSpeed
		{
			get
			{
				return _droneRotationSpeed;
			}
			set
			{
				if ( value >0 && value <= 1 )
				{
					_droneRotationSpeed = value;
				}
			}
		}
		public float DroneTranslationSpeed
		{
			get
			{
				return _droneTranslationSpeed;
			}
			set
			{
				if ( value > 0 && value <= 1 )
				{
					_droneTranslationSpeed = value;
				}
			}
		}
	}
}
