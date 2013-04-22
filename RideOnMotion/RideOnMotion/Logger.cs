using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{
	static class Logger
	{

		/// <summary>
		/// Write a log in a file
		/// </summary>
		/// <param name="toLog"></param>
		public static void Log(String toLog)
		{

			//open the stream file
			StreamWriter log;

			if (!File.Exists("RideOnMotion.log"))
			{
				log = new StreamWriter( "RideOnMotion.log" );
			}
			else
			{
				log = File.AppendText( "RideOnMotion.log" );
			}

			// Write the détail in the file:
			log.WriteLine(DateTime.Now);
			log.WriteLine(toLog);
			log.WriteLine();

			// Close the stream:
			log.Close();
		}
	}
}