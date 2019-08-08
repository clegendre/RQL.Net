using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace RQL.Net
{
    class RQLRequestSignator
    {
        static HMAC CreateHMAC( string authenticationToken )
        {
            byte[] tokenBytes = Encoding.ASCII.GetBytes( authenticationToken );
            HMAC hmac = new HMACMD5( tokenBytes );
            return hmac;
        }

        public static byte[] Sign( string authenticationToken, byte[] contentToHash )
        {
            HMAC hmac = CreateHMAC( authenticationToken );
            return hmac.ComputeHash( contentToHash );
        }

        public static byte[] Sign( string authenticationToken, string contentToHash )
        {
            return Sign( authenticationToken, Encoding.ASCII.GetBytes( contentToHash ) );
        }

        public static byte[] GetMd5( byte[] body )
        {
            MD5 md5Hasher = MD5.Create();
            // Convert the input string to a byte array and compute the hash.

            byte[] data = md5Hasher.ComputeHash( body );
            return data;
        }

        public static string GetMd5Hash( byte[] body )
        {
            var data = GetMd5( body );
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for( int i = 0; i < data.Length; ++i )
            {
                sBuilder.Append( data[i].ToString( "x2" ) );
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string HexDigest( byte[] bytes )
        {
            string result = "";
            foreach( byte b in bytes )
            {
                result += String.Format( "{0:x2}", b );
            }
            return result;
        }
    }
}
