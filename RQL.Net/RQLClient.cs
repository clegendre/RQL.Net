using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RQL.Net
{
    /// <summary>
    /// An extensible RQLClient
    /// </summary>
    public class RQLClient
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<RQLClientConfiguration> _option;

        public RQLClient( HttpClient httpClient, IOptions<RQLClientConfiguration> option )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException( nameof( httpClient ) );
            _option = option ?? throw new ArgumentNullException( nameof( option ) );
            _httpClient.BaseAddress = new System.Uri( option.Value.ServerAddress );
        }

        public virtual async Task<RQLResponse> MakeRequestsAsync( RQLRequestBatch requestBatch )
        {
            if( requestBatch == null )
            {
                throw new ArgumentNullException( nameof( requestBatch ) );
            }

            var content = await CreateHttpContentAsync( requestBatch );
            _httpClient.DefaultRequestHeaders.Date = DateTime.UtcNow;

            CreateAuthorizationHeader( _httpClient, content.Headers );

            var response = await _httpClient.PostAsync( _option.Value.RQLEndPoint, content );

            var resultString = await response.Content.ReadAsStringAsync();

            return new RQLResponse( requestBatch, resultString, response.IsSuccessStatusCode );
        }

        public virtual async Task<HttpContent> CreateHttpContentAsync( RQLRequestBatch requestBatch )
        {
            if( requestBatch == null )
            {
                throw new ArgumentNullException( nameof( requestBatch ) );
            }

            if( requestBatch.Requests.Count == 0 )
            {
                throw new ArgumentException( "There is no RQLRequests to send." );
            }

            var json = SerializeRqlRequestBatch( requestBatch );
            HttpContent content = new StringContent( json, Encoding.UTF8, "application/json" );

            if( requestBatch.HasFiles )
            {
                var multiPartContent = new MultipartFormDataContent();
                multiPartContent.Add( content, "json", "json" );
                foreach( var file in requestBatch.AllFiles )
                {
                    multiPartContent.Add( new ByteArrayContent( file.FileContent ), file.FilePart, file.FilePart );
                }
                content = multiPartContent;
            }


            var body = await content.ReadAsByteArrayAsync();
            var md5 = RQLRequestSignator.GetMd5Hash( body );
            content.Headers.Add( "Content-MD5", md5 );
            content.Headers.ContentLength = body.LongLength;
            return content;
        }

        internal string SerializeRqlRequestBatch( RQLRequestBatch requestBatch )
        {
            if( requestBatch == null )
            {
                throw new ArgumentNullException( nameof( requestBatch ) );
            }
            if( requestBatch.Requests.Count == 0 )
            {
                throw new ArgumentException( "There is no RQLRequests to serizalze." );
            }
            var rqlRequestFormat = requestBatch.Requests.Select( r => new[] { r.Query, r.Parameters ?? new object() } );
            var rqlRequest = Newtonsoft.Json.JsonConvert.SerializeObject( rqlRequestFormat );
            return rqlRequest;
        }

        protected virtual void CreateAuthorizationHeader( HttpClient client, HttpContentHeaders headers )
        {
            string contentToSign = "";
            contentToSign += _option.Value.RQLMethod;
            contentToSign += new Uri( client.BaseAddress, _option.Value.RQLEndPoint ).ToString();
            contentToSign += headers.GetValues( "Content-MD5" ).Single();
            contentToSign += headers.GetValues( "Content-Type" ).Single();
            contentToSign += client.DefaultRequestHeaders.GetValues( "Date" ).Single();

            Console.WriteLine( "Signing request " + contentToSign );
            byte[] hashBytes = RQLRequestSignator.Sign( _option.Value.AuthenticationToken, contentToSign );
            string hashDigest = RQLRequestSignator.HexDigest( hashBytes );

            var authorizationParameter = String.Concat( "runner-", _option.Value.AuthenticationUserId, ":", hashDigest );
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue( "Cubicweb", authorizationParameter );
        }
    }
}
