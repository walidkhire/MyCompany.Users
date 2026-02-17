namespace MyCompany.Users.API.Helpers
{
    public static class ProblemDetailsFactory
    {
        public static object CreateProblem(int status, string title, string detail, string errorCode, string traceId, bool includeStack = false)
        {
            return new
            {
                type = $"https://httpstatuses.com/{status}",
                title,
                status,
                detail,
                errorCode,
                traceId,
                stackTrace = includeStack ? Environment.StackTrace : null
            };
        }
    }
}
