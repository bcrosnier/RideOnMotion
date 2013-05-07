using Microsoft.Kinect;
using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace RideOnMotion.KinectModule
{
	public class CaptionArea : ICaptionArea, INotifyPropertyChanged
    {
        IList<Action> _associateFunctions;
        IList<Point> _points;
        Point _topLeftPoint;
        float _width;
        float _height;
		String _name;

		bool _isActive;

		public event PropertyChangedEventHandler PropertyChanged;

        KinectSensorController.SkelPointToDepthPoint _converter;

        public float Height
        {
            get
            {
                return this._height;
            }
            set
            {
                if ( value != this._height )
                {
                    this._height = value;
                    OnPropertyChanged( "Height" );
                }
            }
        }

        public float Width
        {
            get
            {
                return this._width;
            }
            set
            {
                if ( value != this._width )
                {
                    this._width = value;
                    OnPropertyChanged( "Width" );
                }
            }
        }

        public float X
        {
            get
            {
                return this._topLeftPoint.X;
            }
            set
            {
                if ( value != this._topLeftPoint.X )
                {
                    this._topLeftPoint.X = value;
                    OnPropertyChanged( "X" );
                }
            }
        }

        public float Y
        {
            get
            {
                return this._topLeftPoint.Y;
            }
            set
            {
                if ( value != this._topLeftPoint.Y )
                {
                    this._topLeftPoint.Y = value;
                    OnPropertyChanged( "Y" );
                }
            }
        }

		public String Name
		{
			get
			{
				return this._name;
			}
			private set
			{
				this._name = value;
			}
		}

		public bool IsActive
		{
			get { return _isActive; }
			set { _isActive = value; OnPropertyChanged( "IsActive" ); }
		}
        
		public IReadOnlyList<Action> AssociateFunctions
		{
			get { return _associateFunctions.AsReadOnlyList(); }
		}

        public IReadOnlyList<Point> Points
        {
            get           
            {
                _points = new List<Point>();
                _points.Add(_topLeftPoint);
                Point topRightPoint = new Point(_topLeftPoint.X + _width, _topLeftPoint.Y);
                _points.Add(topRightPoint);
                Point bottomRightPoint = new Point(topRightPoint.X, topRightPoint.Y + _height);
                _points.Add(bottomRightPoint);
                Point bottomLeftPoint = new Point(_topLeftPoint.X, bottomRightPoint.Y);
                _points.Add(bottomLeftPoint);
                return _points.AsReadOnlyList();
            }
        }

        public CaptionArea(String Name, Point topLeftPoint, float width, float height, KinectSensorController.SkelPointToDepthPoint converter )
		{
			_associateFunctions = new List<Action>();

			_name = Name;
            _topLeftPoint = topLeftPoint;
            _width = width;
            _height = height;

            this._converter = converter;

			IsActive = false;
		}

        public void AddFunction(Action action)
        {
            if (action == null) throw new ArgumentNullException("action cannot be null");
            _associateFunctions.Add(action);
        }

        public void RemoveFunction(Action action)
        {
            if (action == null) throw new ArgumentNullException("action cannot be null");
            _associateFunctions.Remove(action);
        }

        public void CheckPosition(Joint joint)
        {
            DepthImagePoint depthPoint;

            if ( _converter == null )
            {
                // This shouldn't happen !
                depthPoint = new DepthImagePoint() { X = (int)joint.Position.X, Y = (int)joint.Position.Y, Depth = (int)joint.Position.Z };
            }
            else
            {
                depthPoint = _converter( joint.Position );
            }

            if ( depthPoint.X > _topLeftPoint.X && depthPoint.X < _topLeftPoint.X + _width && depthPoint.Y > _topLeftPoint.Y && depthPoint.Y < _topLeftPoint.Y + _height && ( joint.TrackingState == JointTrackingState.Tracked || joint.TrackingState == JointTrackingState.Inferred ) )
            {
				if ( IsActive == false )
				{
                    Logger.Instance.NewEntry( LogLevel.Trace, CKTraitTags.ARDrone, joint.JointType.ToString() + " activated " + this.Name );
                    //il va y avoir un probleme d'appel repete des fonctions associees
                    foreach ( Action action in _associateFunctions )
                    {
                        action();
                    }
				}
				IsActive = true;
            }
			else if( IsActive == true )
			{
				IsActive = false;
				//Logger.Instance.NewEntry( LogLevel.Trace, CKTraitTags.ARDrone, joint.JointType.ToString() + " not on " + this.Name );
			}
        }

		protected void OnPropertyChanged( string name )
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if( handler != null )
			{
				handler( this, new PropertyChangedEventArgs( name ) );
			}
		}
    }
}
