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

		static Stream memoryStick = new MemoryStream();
		StreamWriter memoryStickWriter = new StreamWriter( memoryStick );
		//FileStream fs = File.Create( "test.txt" );
		
		
        public void writeLog( String Message )
        {
			memoryStickWriter.Write( DateTime.Now + " : " );
			memoryStickWriter.WriteLine( Message );
			memoryStickWriter.Flush();
		}

        public String readLog()
        {
			memoryStick.Seek( 0, SeekOrigin.Begin );
			StreamReader memoryStickReader = new StreamReader( memoryStick );
			String something = memoryStickReader.ReadLine();
            return something;
        }
    }
}