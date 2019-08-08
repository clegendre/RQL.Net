using Microsoft.Extensions.Options;
using NUnit.Framework;
using RQL.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;
namespace Tests
{
    public class RQLClientTests
    {
        [Test]
        public async Task CreateRqlRequest()
        {
            var rqlRequest = RQLRequestBuilder
                .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s, U email %(user_email)s" )
                .WithParam( "user_name", "MyUserName" )
                .WithParam( "user_email", "MyEmail@Code.com" )
                .Build();

            var rqlClient = CreateRqlClient();
            var httpContent = await rqlClient.CreateHttpContentAsync( rqlRequest );
            var httpContentString = await httpContent.ReadAsStringAsync();
            var httpContentOutFilePath = TestHelper.TestProjectFolder.Combine( $"Out/basic.txt" );
            await File.WriteAllTextAsync( httpContentOutFilePath, httpContentString );
        }

        [Test]
        public void CreateRqlRequestWithMissingParameterInQuery()
        {
            Assert.Throws<ArgumentException>( () =>
                RQLRequestBuilder
                    .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s" )
                        .WithParam( "user_name", "MyUserName" )
                        .WithParam( "user_email", "MyEmail@Code.com" )
                    .Build()
            );
        }

        [Test]
        public void CreateRqlRequestWithMissingFileInQuery()
        {
            Assert.Throws<ArgumentException>( () =>
                RQLRequestBuilder
                    .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s" )
                        .WithParam( "user_name", "MyUserName" )
                        .WithFile( "file", Array.Empty<byte>() )
                    .Build()
            );
        }



        [Test]
        public void CreateRqlBatchRequests()
        {
            var batch = RQLRequestBuilder
                .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s" )
                    .WithParam( "user_name", "MyUserName" )
                .AddNewQuery( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s" )
                    .WithParam( "user_name", "MyUserName2" )
                .Build();

            Assert.That( batch.Requests.Count, Is.EqualTo( 2 ) );
        }


        [Test]
        [TestCase( "Light.xml" )]
        public async Task CreateRqlRequestWithFiles( string fileName )
        {
            var file = TestHelper.TestProjectFolder.Combine( $"In/{fileName}" );
            var fileContent = await File.ReadAllBytesAsync( file );
            var rqlRequest = RQLRequestBuilder
                .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s, A file_for %(file)s" )
                .WithParam( "user_name", "MyUserName" )
                .WithFile( "file", fileContent )
                .Build();

            var rqlClient = CreateRqlClient();
            var httpContent = await rqlClient.CreateHttpContentAsync( rqlRequest );
            var httpContentString = await httpContent.ReadAsStringAsync();
            var httpContentOutFilePath = TestHelper.TestProjectFolder.Combine( $"Out/{fileName}.txt" );
            await File.WriteAllTextAsync( httpContentOutFilePath, httpContentString );
        }


        [Test]
        [TestCase( "Light.xml" )]
        public void CreateRqlBatchRequestsWithFile( string fileName )
        {
            var file = TestHelper.TestProjectFolder.Combine( $"In/{fileName}" );
            var fileContent = File.ReadAllBytes( file );
            var batch = RQLRequestBuilder
                .Query( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s" )
                    .WithParam( "user_name", "MyUserName" )
                .AddNewQuery( "Any A WHERE A is AuthToken, A token_for_user U, U login %(user_name)s, A file_for %(file)s" )
                    .WithParam( "user_name", "MyUserName2" )
                    .WithFile( "file", fileContent )
                .Build();

            Assert.That( batch.Requests.Count, Is.EqualTo( 2 ) );
            Assert.That( batch.Requests[1].Files.Count, Is.EqualTo( 1 ) );
        }

        private static RQLClient CreateRqlClient()
        {
            var option = Options.Create( new RQLClientConfiguration
            {
                ServerAddress = "https://some.rql.endpoint.net/",
                RQLEndPoint = "rqlio/1.0",
                RQLMethod = "POST",
                AuthenticationUserId = 123456,
                AuthenticationToken = "SomeToken",
            } );
            var httpClient = new HttpClient
            {
                BaseAddress = new System.Uri( option.Value.ServerAddress )
            };
            httpClient.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue( "application/json" ) );

            return new RQLClient( httpClient, option );
        }

        static string WriteResponse( HttpWebRequest request )
        {
            try
            {
                using( var response = request.GetResponse() )
                {
                    var responseStream = response.GetResponseStream();
                    using( var streamReader = new StreamReader( responseStream ) )
                    {
                        var result = streamReader.ReadToEnd();
                        return result;
                    }
                }
            }
            catch( WebException ex )
            {
                HttpWebResponse response = ex.Response as HttpWebResponse;
                using( var streamReader = new StreamReader( response.GetResponseStream() ) )
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
        }
    }
}
