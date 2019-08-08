namespace RQL.Net
{
    public class RQLClientConfiguration
    {
        public string ServerAddress { get; set; }
        public string AuthenticationToken { get; set; }
        public string RQLEndPoint { get; set; } = "rqlio/1.0";
        public string RQLMethod { get; set; } = "POST";
        public int AuthenticationUserId { get; set; }
    }
}
