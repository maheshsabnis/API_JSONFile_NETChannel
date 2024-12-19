namespace API_JSONFile_NETChannel.Middlewares
{
    public class ExecutionException (RequestDelegate _next)
    { 
       
        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);   
            }
            catch (Exception ex)
            {
                await ctx.Response.WriteAsJsonAsync(ex.Message);
            }
        }
    }


   public static class AppExceptionMiddleware
   {
       public static void UseAppException(this IApplicationBuilder builder)
       { 
            builder.UseMiddleware<ExecutionException>();         
       }
   }
}
