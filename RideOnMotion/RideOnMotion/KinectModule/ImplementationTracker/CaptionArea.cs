using Microsoft.Kinect;
using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.KinectModule
{
    public class CaptionArea : ICaptionArea
    {
        IList<Action> _associateFunctions;
        IList<Point> _points;
        Point _topLeftPoint;
        float _length;
        float _width;
        
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
                Point topRightPoint = new Point(_topLeftPoint.X + _length, _topLeftPoint.Y);
                _points.Add(topRightPoint);
                Point bottomRightPoint = new Point(topRightPoint.X, topRightPoint.Y + _width);
                _points.Add(bottomRightPoint);
                Point bottomLeftPoint = new Point(_topLeftPoint.X, bottomRightPoint.Y);
                _points.Add(bottomLeftPoint);
                return _points.AsReadOnlyList();
            }
        }

		public CaptionArea(Point topLeftPoint, float length, float width)
		{
            if (topLeftPoint == null) throw new ArgumentNullException("point cannot be null");
			_associateFunctions = new List<Action>();
            _topLeftPoint = topLeftPoint;
            _length = length;
            _width = width;
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
            if (joint.Position.X > _points[0].X && joint.Position.X < _points[1].X && joint.Position.Y > _points[2].Y && joint.Position.Y < _points[0].Y)
            {
                foreach (Action action in _associateFunctions)
                {
                    action();
                }
            }
        }
    }
}
