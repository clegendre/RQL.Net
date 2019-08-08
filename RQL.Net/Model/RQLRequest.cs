using System;
using System.Collections.Generic;
using System.IO;

namespace RQL.Net
{
    public class RQLRequest
    {
        private List<RQLRequestFile> _files;
        private readonly int _requestIndex;
        private readonly Dictionary<string, object> _parameters;

        internal RQLRequest( int requestIndex, string query, IDictionary<string, object> parameters = null )
        {
            Query = query ?? throw new ArgumentNullException( nameof( query ) );
            _parameters = parameters != null ? new Dictionary<string, object>( parameters ) : new Dictionary<string, object>();
            _requestIndex = requestIndex;
        }

        /// <summary>
        /// Gets the RQL Query
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Gets the parameters
        /// </summary>
        public IReadOnlyDictionary<string, object> Parameters => _parameters;

        public IReadOnlyCollection<RQLRequestFile> Files
        {
            get
            {
                if( _files == null ) return Array.Empty<RQLRequestFile>();
                return _files;
            }
        }

        internal string AddFile( byte[] body )
        {
            if( body == null )
            {
                throw new ArgumentNullException( nameof( body ) );
            }
            if( _files == null )
                _files = new List<RQLRequestFile>();

            var file = new RQLRequestFile( _requestIndex, _files.Count, body );
            _files.Add( file );
            return file.FilePart;
        }

        internal void AddParameter( string variableName, object value )
        {
            if( _parameters.ContainsKey( variableName ) )
                throw new ArgumentException( $"A variable is already defined with the name {variableName}" );

            _parameters.Add( variableName, value );
        }
    }

}
