using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RideOnMotion;

namespace RideOnMotion.test
{
	[TestFixture]
    class LoggerTest
    {
		Logger logger = new Logger();
		String thisIsAString = "Hello, I'm testing this shit v2!";

		[Test]
		public void writeAndReadTest()
		{
			Console.WriteLine( "Entered : " + DateTime.Now + " : " + thisIsAString );
			logger.writeLog( thisIsAString );
			Console.WriteLine( "Result : " + logger.readLog() );
			Console.WriteLine();

			//StringAssert. DateTime.Now + "";
			StringAssert.Contains( DateTime.Now + " : " +thisIsAString, logger.readLog() );
		}

    }
}
