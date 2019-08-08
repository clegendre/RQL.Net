using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RQL.Net
{
    /// <summary>
    /// The RQL Request builder
    /// </summary>
    public class RQLRequestBuilder
    {
        private readonly List<RQLRequest> _requests;

        internal RQLRequestBuilder()
        {
            _requests = new List<RQLRequest>();
        }

        /// <summary>
        /// Creates a first RQL Query
        /// </summary>
        /// <param name="query">The RQL Query</param>
        /// <param name="parameters">An optional bag of parameters</param>
        /// <returns></returns>
        public static RQLRequestBuilder Query( string query, IDictionary<string, object> parameters = null )
        {
            if( query == null )
            {
                throw new ArgumentNullException( nameof( query ) );
            }

            var builder = new RQLRequestBuilder();
            return builder.AddNewQuery( query, parameters );
        }

        /// <summary>
        /// Adds a new RQLQuery to this builder instance. Usefull for batching queries.
        /// </summary>
        /// <param name="query">The RQL Query</param>
        /// <param name="parameters">An optional bag of parameters</param>
        /// <returns></returns>
        public RQLRequestBuilder AddNewQuery( string query, IDictionary<string, object> parameters = null )
        {
            if( query == null )
            {
                throw new ArgumentNullException( nameof( query ) );
            }

            var request = new RQLRequest( _requests.Count, query, parameters );
            _requests.Add( request );

            return this;
        }

        /// <summary>
        /// Adds a param to the current RQL Query with the given value, which should be JSON serializable easily.
        /// The variable name MUST exists in the RQL Query with a name equals to %(variableName)s.
        /// </summary>
        /// <remarks>
        /// We do not supports param index now, so parameters must be defined in the RQL Query by name.
        /// </remarks>
        /// <param name="variableName">The name of the variable in the RQL query</param>
        /// <param name="value">A value. Can be null.</param>
        /// <returns></returns>
        public RQLRequestBuilder WithParam( string variableName, object value )
        {
            if( variableName == null )
            {
                throw new ArgumentNullException( nameof( variableName ) );
            }
            if( _requests.Count == 0 ) throw new ArgumentException( "You must first add a query" );

            var request = _requests[_requests.Count - 1];
            if( request.Query.IndexOf( variableName ) == -1 )
            {
                throw new ArgumentException( $"The variable name should exists in the RQLQuery with value: %({variableName})s" );
            }

            request.AddParameter( variableName, value );
            return this;
        }

        /// <summary>
        /// Adds a file to the current RQL Query with the given fileName.
        /// The file name MUST exists in the RQL Query with a name equals to %(fileName)s
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public RQLRequestBuilder WithFile( string fileName, byte[] body )
        {
            if( fileName == null )
            {
                throw new ArgumentNullException( nameof( fileName ) );
            }

            if( _requests.Count == 0 ) throw new ArgumentException( "You must first add a query" );

            var fileContent = new RQLFilePart( fileName, body );
            var request = _requests[_requests.Count - 1];
            if( request.Query.IndexOf( fileName ) == -1 )
            {
                throw new ArgumentException( $"The file name should exists in the RQLQuery with value: %({fileName})s" );
            }
            string filePartIndex = request.AddFile( fileContent.Content );
            WithParam( fileContent.PropertyName, filePartIndex );

            return this;
        }

        /// <summary>
        /// Builds the <see cref="RQLRequestBatch"/>
        /// </summary>
        /// <returns></returns>
        public RQLRequestBatch Build()
        {
            return new RQLRequestBatch( _requests );
        }

    }
}
