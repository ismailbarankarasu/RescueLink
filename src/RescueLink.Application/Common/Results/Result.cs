namespace RescueLink.Application.Common.Results;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException(
                "A successful result cannot contain an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException(
                "A failed result must contain an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result<TValue> Success<TValue>(TValue value)
    {
        return new Result<TValue>(value, true, Error.None);
    }

    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(
            default,
            false,
            error);
    }
}