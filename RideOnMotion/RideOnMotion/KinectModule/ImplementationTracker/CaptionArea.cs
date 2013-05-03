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
		bool _isActive;

		public event PropertyChangedEventHandler PropertyChanged;

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

		public CaptionArea(Point topLeftPoint, float width, float height)
		{
			_associateFunctions = new List<Action>();
            _topLeftPoint = topLeftPoint;
            _width = width;
            _height = height;
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
            if( joint.Position.X > _topLeftPoint.X && joint.Position.X < _topLeftPoint.X + _width && joint.Position.Y > _topLeftPoint.Y && joint.Position.Y < _topLeftPoint.Y + _height )
            {
				IsActive = true;
				//il va y avoir un probleme d'appel repete des fonctions associees
                foreach (Action action in _associateFunctions)
                {
                    action();
                }
            }
			else if( IsActive == true )
			{
				IsActive = false;
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
