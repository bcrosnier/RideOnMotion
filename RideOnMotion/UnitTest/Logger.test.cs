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

		[Test]
		public void writeTest()
		{
			logger.writeLog( "Hello, I'm testing this shit !" );
		}

		[Test]
		public void readTest()
		{
			String stringToTest = logger.readLog();
			Console.WriteLine( stringToTest );
		}

    }
}
