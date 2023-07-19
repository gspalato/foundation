namespace Reality.Services.Identity
{
    public class ErrorFilter : IErrorFilter
    {
        private ILogger<ErrorFilter> Logger { get; }

        public ErrorFilter(ILogger<ErrorFilter> logger)
        {
            Logger = logger;
        }

        public IError OnError(IError error)
        {
            Logger.LogError(error.Exception, error.Exception?.Message);
            return error;
        }
    }
}