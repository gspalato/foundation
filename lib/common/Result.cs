namespace Foundation.Common;

public class Result<T, E>
{
    private bool Success { get; set; } = false;

    public T Value { get; set; } = default!;

    public E Error { get; set; } = default!;


    public Result(bool success)
    {
        Success = success;
    }

    public Result(T value)
    {
        Success = true;
        Value = value;
    }

    public Result(E error)
    {
        Success = false;
        Error = error;
    }


    public void Deconstruct(out T value, out E error)
    {
        value = Value!;
        error = Error!;
    }


    public static implicit operator bool(Result<T, E> result)
    {
        return result.Success;
    }

    public static implicit operator T(Result<T, E> result)
    {
        return result.Value;
    }

    public static implicit operator E(Result<T, E> result)
    {
        return result.Error;
    }
}

public class ValueResult<T, E> : Result<T, E>
{
    public ValueResult(T value) : base(value) { }
}

public class ErrorResult<T, E> : Result<T, E>
{
    public ErrorResult(E error) : base(error) { }
}