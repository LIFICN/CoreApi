using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;

namespace CommonExtensions;

public static class SwaggerExtension
{
    public static void AddSwagger(this IServiceCollection services, string apiName)
    {
        #region Swagger多版本控制
        services.AddApiVersioning(option =>
        {
            // 可选，为true时API返回支持的版本信息
            option.ReportApiVersions = true;
            // 请求中未指定版本时为默认
            option.DefaultApiVersion = ApiVersion.Default;
            // 不提供版本时，默认为1.0
            option.AssumeDefaultVersionWhenUnspecified = true;
        })
        .AddVersionedApiExplorer(option => option.GroupNameFormat = "'v'VVV");
        #endregion

        #region 注册Swagger服务
        services.AddSwaggerGen(c =>
        {
            #region 启用swagger Bearer Token验证功能
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "格式:Bearer +空格+token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(
              new OpenApiSecurityRequirement()
              {
                    {
                      new OpenApiSecurityScheme()
                        {
                          Reference = new OpenApiReference()
                          {
                            Id = "Bearer", Type = ReferenceType.SecurityScheme
                          }
                        },
                        Array.Empty<string>()
                    }
              });
            #endregion

            #region 使用反射获取xml文件。并构造出文件的路径
            var xmlFile = $"{apiName}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            // 启用xml注释. 该方法第二个参数启用控制器的注释，默认为false.
            c.IncludeXmlComments(xmlPath, true);
            #endregion

            #region 多版本控制
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var item in provider.ApiVersionDescriptions)
            {
                // 添加文档信息
                c.SwaggerDoc(item.GroupName, new OpenApiInfo
                {
                    Title = "CoreApi",
                    Version = item.ApiVersion.ToString(),
                    Description = "CoreApi-Dev",
                    Contact = new OpenApiContact
                    {
                        Name = "LIFI",
                        Email = "123456@gmail.com"
                    }
                });
            }
            #endregion

            //去掉自带api-version,治疗强迫症
            c.OperationFilter<RemoveVersionParameterOperationFilter>();
        });
        #endregion
    }

    public static void UseSawggerAndUI(this IApplicationBuilder app, string routePrefix = "ApiDocs")
    {
        #region 启用Swagger中间件
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DocExpansion(DocExpansion.None); //None:折叠所有Tag
            c.DefaultModelsExpandDepth(0); // 是否显示Models -1:不显示 0:折叠 1:展开
            c.RoutePrefix = routePrefix;  //swagger 路由
            var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var item in provider.ApiVersionDescriptions)
            {
                c.SwaggerEndpoint($"/swagger/{item.GroupName}/swagger.json", item.GroupName);
            }
        });
        #endregion
    }
}

public class RemoveVersionParameterOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null || operation.Parameters.Count == 0)
            return;

        //去掉自带api-version,治疗强迫症
        var versionParameter = operation.Parameters.FirstOrDefault(d => d.Name.ToLower().Equals("api-version") && d.In == ParameterLocation.Query);
        if (versionParameter != null) operation.Parameters.Remove(versionParameter);
    }
}
