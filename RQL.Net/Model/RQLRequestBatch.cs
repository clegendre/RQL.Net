using System.Collections.Generic;
using System.Linq;

namespace RQL.Net
{
    public class RQLRequestBatch
    {
        internal RQLRequestBatch( IReadOnlyList<RQLRequest> requests )
        {
            Requests = requests ?? throw new System.ArgumentNullException( nameof( requests ) );
            AllFiles = requests.SelectMany( f => f.Files ).ToArray();
        }
        public IReadOnlyList<RQLRequest> Requests { get; }
        public IReadOnlyList<RQLRequestFile> AllFiles { get; }
        public bool HasFiles => AllFiles.Count > 0;
    }
}
