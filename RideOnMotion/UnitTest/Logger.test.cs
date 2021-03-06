﻿using System;
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
		public void logtest()
		{
			IDefaultActivityLogger Logger = new DefaultActivityLogger();
			Logger.Tap.Register( new ListImpl() );

			var tag1 = ActivityLogger.RegisteredTags.FindOrCreate( "Product" );
			var tag2 = ActivityLogger.RegisteredTags.FindOrCreate( "Sql" );
			var tag3 = ActivityLogger.RegisteredTags.FindOrCreate( "Combined Tag|Sql|Engine V2|Product" );

			Logger.UnfilteredLog( tag1, LogLevel.Fatal, "Fatal Log ", DateTime.UtcNow );
			Logger.UnfilteredLog( tag1, LogLevel.Fatal, "Fatal Log ", DateTime.UtcNow );
			Logger.UnfilteredLog( tag3, LogLevel.Trace, "Trace Log ", DateTime.UtcNow );

			Console.WriteLine( Logger.Tap.RegisteredSinks.OfType<ListImpl>().Single().LoggerContent );
			Logger.UnfilteredLog( tag2, LogLevel.Info, "Info Log ", DateTime.UtcNow );
			Console.WriteLine( Logger.Tap.RegisteredSinks.OfType<ListImpl>().Single().LoggerContent );
		}
		[Test]
		public void LogTest()
		{
			Logger.Instance.NewEntry( LogLevel.Info, CKTraitTags.Application, "Try again" );
			Console.WriteLine( Logger.Instance.Output() );
		}
	}
}
