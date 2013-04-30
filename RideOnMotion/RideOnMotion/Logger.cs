using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace RideOnMotion
{

    /// <summary>
    /// First version of the logger on 23.04.2013
    /// Wait for the new version, right now I don't have time to make something cool.
    /// </summary>
    public class Logger
    {
		private static Logger instance = new Logger();
		public static Logger Instance
		{
			get
			{
				if ( instance == null )
				{
					instance = new Logger();
				}
				return instance;
			}
		}

		IDefaultActivityLogger _logger;

		public List<CKTrait> Tags = new List<CKTrait>();
        public event EventHandler<String> NewLogStringReady;

		/// <summary>
		/// Start the logger and use the string implementation
		/// </summary>
		private Logger()
		{
			_logger = new DefaultActivityLogger();
			_logger.Tap.Register( new StringImpl() );
			
		}


		/// <summary>
		/// Log a new line in the logger
		/// </summary>
		/// <param name="logLevel">The level of the log</param>
		/// <param name="tag">The tags the log should be associated with, using CKTraitTags extensions</param>
		/// <param name="text">The text to log</param>
		public void NewEntry(LogLevel logLevel, CKTrait tag, String text)
		{
			_logger.UnfilteredLog( tag, logLevel, text, DateTime.UtcNow );

            if ( NewLogStringReady != null )
            {
                NewLogStringReady( this, Output() );
            }
		}

		/// <summary>
		/// Output the current logger in it's integrity
		/// </summary>
		/// <returns>the logger output as a String</returns>
		public String Output()
		{
			return _logger.Tap.RegisteredSinks.OfType<StringImpl>().Single().Writer.ToString();
		}

    }

	public class CKTraitTags
	{
		public static CKTrait None = ActivityLogger.RegisteredTags.FindOrCreate( "None" );
		public static CKTrait UnknownSensor = ActivityLogger.RegisteredTags.FindOrCreate( "UnknownSensor" );
		public static CKTrait UnknownDrone = ActivityLogger.RegisteredTags.FindOrCreate( "UnknownDrone" );
		public static CKTrait Application = ActivityLogger.RegisteredTags.FindOrCreate( "Application" );
		public static CKTrait Kinect = ActivityLogger.RegisteredTags.FindOrCreate( "Kinect" );
		public static CKTrait ARDrone = ActivityLogger.RegisteredTags.FindOrCreate( "ARDrone" );
	}

	public class StringImpl : IActivityLoggerSink
	{
		public StringWriter Writer { get; private set; }

		public StringImpl()
		{
			Writer = new StringWriter();
		}

		public void OnEnterLevel( CKTrait trait, LogLevel level, string text, DateTime time )
		{
			time = time.ToLocalTime();
			Writer.WriteLine();
			Writer.Write( time.ToString( "H:mm:ss" ) + " [" + level.ToString() + " : "+ trait.ToString() +"] " + text );
		}

		public void OnContinueOnSameLevel( CKTrait trait, LogLevel level, string text, DateTime time )
		{
			time = time.ToLocalTime();
			Writer.WriteLine();
			Writer.Write( time.ToString( "H:mm:ss" ) + " [" + level.ToString() + " : " + trait.ToString() + "] " + text );
		}

		public void OnLeaveLevel( LogLevel level )
		{
			Writer.Flush();
		}

		public void OnGroupOpen( IActivityLogGroup g )
		{
			Writer.WriteLine();
			Writer.Write( new String( '+', g.Depth ) );
			Writer.Write( "{1} ({0})", g.GroupLevel, g.GroupText );
		}

		public void OnGroupClose( IActivityLogGroup g, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
		{
			Writer.WriteLine();
			Writer.Write( new String( '-', g.Depth ) );
			Writer.Write( String.Join( ", ", conclusions.Select( c => c.Text ) ) );
		}
	}
}