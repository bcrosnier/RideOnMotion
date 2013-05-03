using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RideOnMotion.KinectModule;
using Microsoft.Kinect;

namespace UnitTest.KinectModuleTests.TrackerTests
{
	[TestFixture]
	public class CaptionAreaTests
	{
		[Test]
		public void CaptionAreaPointTest()
		{
			CaptionArea mockCaptionArea = new CaptionArea( new Point( 0, 0 ), 10, 10, null );
			IReadOnlyList<Point> listOfPoint = mockCaptionArea.Points;

			Assert.That( listOfPoint[0].X, Is.EqualTo( 0 ) );
			Assert.That( listOfPoint[1].X, Is.EqualTo( 10 ) );
			Assert.That( listOfPoint[2].X, Is.EqualTo( 10 ) );
			Assert.That( listOfPoint[3].X, Is.EqualTo( 0 ) );
			Assert.That( listOfPoint[0].Y, Is.EqualTo( 0 ) );
			Assert.That( listOfPoint[1].Y, Is.EqualTo( 0 ) );
			Assert.That( listOfPoint[2].Y, Is.EqualTo( 10 ) );
			Assert.That( listOfPoint[3].Y, Is.EqualTo( 10 ) );
		}

		[Test]
		public void CaptionAreaCallFunctionTests()
		{
			int i = 0;
            CaptionArea mockCaptionArea = new CaptionArea( new Point( 0, 0 ), 10, 10, null );
			Action function = () => { i++; };
			mockCaptionArea.AddFunction( function );
			Joint joint = new Skeleton().Joints[0];

			mockCaptionArea.CheckPosition( joint );
			Assert.That( i, Is.EqualTo( 0 ) );

			joint.Position = new SkeletonPoint() { X = 2, Y = 2};

			mockCaptionArea.CheckPosition( joint );
			Assert.That( i, Is.EqualTo( 1 ) );

			mockCaptionArea.CheckPosition( joint );
			Assert.That( i, Is.EqualTo( 2 ) );

			joint.Position = new SkeletonPoint() { X = 12, Y = 12 };

			mockCaptionArea.CheckPosition( joint );
			Assert.That( i, Is.EqualTo( 2 ) );

			mockCaptionArea.RemoveFunction( function );

			mockCaptionArea.CheckPosition( joint );
			Assert.That( i, Is.EqualTo( 2 ) );

		}
	}
}
