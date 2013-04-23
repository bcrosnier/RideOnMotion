using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{

    /// <summary>
    /// First version of the logger on 23.04.2013
    /// Wait for the new version, by now, I don't have the time to make something cool..
    /// </summary>
    static class Logger
    {

        public static void writeLog( String toLog )
        {

            StreamWriter log;

            if( !File.Exists( "RideOnMotion.log" ) )
            {
                log = new StreamWriter( "RideOnMotion.log" );
            }
            else
            {
                log = File.AppendText( "RideOnMotion.log" );
            }

            log.WriteLine( DateTime.Now );
            log.WriteLine( toLog );
            log.WriteLine();

            log.Close();
        }

        public static String readLog()
        {
            StreamReader logReader;

            if( !File.Exists( "RideOnMotion.log" ) )
            {
                logReader = new StreamReader( "RideOnMotion.log" );
                String something = logReader.ReadToEnd();
                return something;
            }
            else
            {
                return String.Empty;
            }

        }
    }
}