using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CK.Core;
using System.IO;

namespace RideOnMotion.test
{
	[TestFixture]
	class LoggerTest
	{
		[Test]
		void logtest()
		{
			IDefaultActivityLogger Logger = new DefaultActivityLogger();
			Logger.Tap.Register( new StringImpl() );

			var tag1 = ActivityLogger.RegisteredTags.FindOrCreate( "Product" );
			var tag2 = ActivityLogger.RegisteredTags.FindOrCreate( "Sql" );
			var tag3 = ActivityLogger.RegisteredTags.FindOrCreate( "Combined Tag|Sql|Engine V2|Product" );

			Logger.UnfilteredLog( tag1, LogLevel.Fatal, "Fatal Biatch ", DateTime.UtcNow );
			Logger.UnfilteredLog( tag1, LogLevel.Fatal, "Fatal Biatch ", DateTime.UtcNow );
			Logger.UnfilteredLog( tag3, LogLevel.Trace, "Trace Biatch ", DateTime.UtcNow );

			Console.WriteLine( Logger.Tap.RegisteredSinks.OfType<StringImpl>().Single().Writer );
			Logger.UnfilteredLog( tag2, LogLevel.Info, "Info Biatch ", DateTime.UtcNow );
			Console.WriteLine( Logger.Tap.RegisteredSinks.OfType<StringImpl>().Single().Writer );
		}
		[Test]
		public void LogTest()
		{
			Logger l = new Logger();
			l.StartLogger();
			l.RegisterTag( "Test" );
			l.NewEntry( LogLevel.Info, l.Tags[0], "Try again" );
			Console.WriteLine( l.Output() );
		}
	}
}
