using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion.Utilities
{
	class InputCapabilities
	{
		int Id = 0;
		Type type = null;
		string Name = "None";
		// Does using this input prevent from using another one ?
		// null if none, reference of the input if the is one
		InputCapabilities ExclusiveWith = null;
	}
}
