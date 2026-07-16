namespace ASP_MessageBoard.Common.Exceptions
{
    public sealed class PostAccessDeniedException : Exception
    {
        public PostAccessDeniedException()
            : base("您沒有權限操作這篇文章。") { }

        public PostAccessDeniedException(Exception innerException)
            : base("您沒有權限操作這篇文章。", innerException) { }
    }
}
