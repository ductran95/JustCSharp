namespace JustCSharp.Data.Requests
{
    public class SortRequest: IRequest
    {
        public string Field { get; set; }
        public bool Asc { get; set; }
    }
}