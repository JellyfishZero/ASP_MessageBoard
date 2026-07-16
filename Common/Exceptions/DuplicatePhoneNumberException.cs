namespace ASP_MessageBoard.Common.Exceptions;

public sealed class DuplicatePhoneNumberException : Exception
{
    public DuplicatePhoneNumberException()
        : base("此手機號碼已經註冊。")
    {
    }

    public DuplicatePhoneNumberException(Exception innerException)
        : base("此手機號碼已經註冊。", innerException)
    {
    }
}
