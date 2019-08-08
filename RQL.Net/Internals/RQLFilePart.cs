using System;

namespace RQL.Net
{
    class RQLFilePart
    {
        public RQLFilePart( string fileName, byte[] body )
        {
            PropertyName = fileName ?? throw new ArgumentNullException( nameof( fileName ) );
            Content = body ?? throw new ArgumentNullException( nameof( body ) );
        }

        public string PropertyName { get; set; }
        public byte[] Content { get; set; }
    }

}
