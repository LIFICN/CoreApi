using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi.Filters;

public class ModelActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        #region Action执行前
        if (!context.ModelState.IsValid)
        {
            string errorMsg = string.Empty;
            foreach (var item in context.ModelState.Values)
            {
                var modelError = item.Errors.FirstOrDefault(d => !string.IsNullOrWhiteSpace(d.ErrorMessage));
                if (modelError != null)
                {
                    errorMsg += $"{modelError.ErrorMessage}  ";
                }
            }

            context.Result = new BadRequestObjectResult(new { error = errorMsg });
            return;
        }
        #endregion

        await next();

        #region Action执行后
        #endregion
    }
}
