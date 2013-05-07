using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RideOnMotion
{
	public interface IViewModel : INotifyPropertyChanged
	{
		 new event PropertyChangedEventHandler PropertyChanged;

		 void OnNotifyPropertyChange( string propertyName );
	}
}
