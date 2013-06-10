using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Utilities
{
	public class MacAddress
	{
		public static String GetWifiMacAddress()
		{
			foreach ( NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces() )
			{

				if ( nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 
					&& nic.OperationalStatus == OperationalStatus.Up 
					&& ( !nic.Description.Contains( "Virtual" ) && !nic.Description.Contains( "Pseudo" ) ) )
				{
					if ( nic.GetPhysicalAddress().ToString() != "" )
					{
						return FromMacWithoutSeparatorToMacWithSeparator(nic.GetPhysicalAddress().ToString());
					}
				}
			}
			return null;
		}


		static string FromMacWithoutSeparatorToMacWithSeparator( string ToTransform )
		{
			ToTransform = ToTransform.ToUpperInvariant();
			var list = Enumerable
				.Range( 0, ToTransform.Length / 2 )
				.Select( i => ToTransform.Substring( i * 2, 2 ) );
			return string.Join( ":", list );
		}
	}
}
