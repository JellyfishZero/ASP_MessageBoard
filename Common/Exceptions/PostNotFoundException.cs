namespace ASP_MessageBoard.Common.Exceptions
{
    public sealed class PostNotFoundException : Exception
    {
        public PostNotFoundException()
            : base("找不到指定的文章。") { }
    }
}
