using System;

namespace RQL.Net
{
    public class RQLRequestFile
    {
        public static readonly string FilePathPrefix = "__f";

        internal RQLRequestFile( int requestIndex, int filePartIndex, byte[] fileContent )
        {
            FileContent = fileContent ?? throw new ArgumentNullException( nameof( fileContent ) );
            FilePart = String.Concat( FilePathPrefix, requestIndex, "_", filePartIndex );
        }

        public string FilePart { get; }

        public byte[] FileContent { get; }
    }

}
