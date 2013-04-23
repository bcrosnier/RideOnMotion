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
    public class Logger
    {

        MemoryStream memoryStick = new MemoryStream();

        public void writeLog( String toLog )
        {

            StreamWriter log;

            log = new StreamWriter( this.memoryStick );

            log.WriteLine( DateTime.Now + " : " );
            log.WriteLine( toLog );

            log.Close();
        }

        public String readLog()
        {
            StreamReader logReader;

            logReader = new StreamReader( this.memoryStick );
            String something = logReader.ReadToEnd();
            return something;
        }
    }
}