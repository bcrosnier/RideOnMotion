using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using CK.Core;

namespace RideOnMotion.Utilities
{
    /// <summary>
    /// Logger client that manages strings in a string collection.
    /// </summary>
    public class StringCollectionLoggerClient : IActivityLoggerClient
    {
        private int _maxLogEntries;
        private Collection<string> _outputCollection;

        /// <summary>
        /// Create a new logger client that will add strings to a collection, and clear old entries whenever a maximum number of entries is reached.
        /// </summary>
        /// <param name="targetCollection">Collection to use</param>
        /// <param name="maxLogEntries">Maximum number of entries</param>
        public StringCollectionLoggerClient( Collection<string> targetCollection, int maxLogEntries )
        {
            if ( targetCollection == null ) throw new ArgumentNullException( "Output TextWriter must exist." );
            _outputCollection = targetCollection;
            _maxLogEntries = maxLogEntries;
        }

        private void AddString( string str )
        {
            Invoke( () =>
            {
                if ( _outputCollection.Count >= _maxLogEntries )
                {
                    _outputCollection.RemoveAt( 0 );
                }
                _outputCollection.Add( str );
            } );
        }

        public void OnFilterChanged( LogLevelFilter current, LogLevelFilter newValue )
        {
            AddString( "Filter level changed to: " + newValue );
        }

        public void OnGroupClosed( IActivityLogGroup group, ICKReadOnlyList<ActivityLogGroupConclusion> conclusions )
        {
            AddString( "Group closed: " + group.GroupText );
            foreach ( var conclusion in conclusions )
            {
                AddString( "    [" + conclusion.Tag + "] " + conclusion.Text );
            }
        }

        public void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
        {
            AddString( "Group closing: " + group.GroupText );
            foreach ( var conclusion in conclusions )
            {
                AddString( "    [" + conclusion.Tag + "] " + conclusion.Text );
            }
        }

        public void OnOpenGroup( IActivityLogGroup group )
        {
            AddString( "Group open: " + group.GroupText );
        }

        public void OnUnfilteredLog( CKTrait tags, LogLevel level, string text, DateTime logTimeUtc )
        {
            AddString( "[" + logTimeUtc.ToLocalTime().ToString( "HH:mm:ss" ) + "] " + tags.AtomicTraits.First().ToString() + @": [" + level.ToString() + "] " + text );
        }

        private static void Invoke( Action action )
        {
            Dispatcher dispatchObject = System.Windows.Application.Current.Dispatcher;
            if ( dispatchObject == null || dispatchObject.CheckAccess() )
            {
                action();
            }
            else
            {
                dispatchObject.Invoke( action );
            }
        }
    }
}
