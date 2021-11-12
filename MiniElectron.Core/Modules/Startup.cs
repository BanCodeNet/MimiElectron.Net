using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerGen;
using DwFramework.Core;
using DwFramework.Web;

namespace MiniElectron.Core
{
    internal sealed class Startup
    {
        private const string PROJECT_NAME = "MiniElectron.Core";
        private const string PROJECT_DESC = "MiniElectron.Core";
        private const string GLOBAL_ROUTE = "";

        public Startup() { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
            services.AddMvc(options => options.UseRoutePrefix(GLOBAL_ROUTE));
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });
            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options => options.OperationFilter<CustomOperationFilter>());
            services.AddControllers(options =>
            {
                options.Filters.Add<ResultFilter>();
                options.Filters.Add<ExceptionFilter>();
            }).AddJsonOptions(options =>
            {
                //不使用驼峰样式的key
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                //不使用驼峰样式的key
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });
        }

        public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime, IApiVersionDescriptionProvider provider)
        {
            app.UseCors("any");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
                }
            });
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
        {
            private readonly IApiVersionDescriptionProvider _provider;
            public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;
            public void Configure(SwaggerGenOptions options)
            {
                foreach (var description in _provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
                }
            }
            private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
            {
                var info = new OpenApiInfo()
                {
                    Title = PROJECT_NAME,
                    Version = description.ApiVersion.ToString(),
                    Description = PROJECT_DESC
                };
                if (description.IsDeprecated)
                {
                    info.Description += " 方法被弃用.";
                }
                return info;
            }
        }

        internal sealed class CustomOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var apiDescription = context.ApiDescription;
                operation.Deprecated |= apiDescription.IsDeprecated();
                if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();
                foreach (var parameter in operation.Parameters)
                {
                    var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                    if (parameter.Description == null)
                    {
                        parameter.Description = description.ModelMetadata?.Description;
                    }
                    if (parameter.Schema.Default == null && description.DefaultValue != null)
                    {
                        parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());
                    }
                    parameter.Required |= description.IsRequired;
                }
            }
        }

        internal sealed class ResultFilter : IAsyncResultFilter
        {
            public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
            {
                if (context.Result is EmptyResult) context.Result = new ObjectResult(new ResponeBody() { Code = 200, Message = "Success" });
                else if (context.Result is ObjectResult)
                {
                    var result = context.Result as ObjectResult;
                    if (result.Value is ResultInfo) context.Result = new ObjectResult(result.Value);
                    else context.Result = new ObjectResult(new ResponeBody()
                    {
                        Code = 200,
                        Message = "Success",
                        Result = result?.Value
                    });
                }
                _ = await next();
            }
        }

        internal sealed class ExceptionFilter : IAsyncExceptionFilter
        {
            public Task OnExceptionAsync(ExceptionContext context)
            {
                if (context.ExceptionHandled == false)
                {
                    var ex = context.Exception.InnerException != null ? context.Exception.InnerException : context.Exception;
                    context.Result = new ObjectResult(new ResponeBody()
                    {
                        Code = ex is CustomException ? ((CustomException)ex).Code : 400,
                        Message = ex is CustomException ? ((CustomException)ex).Message : ex.Message,
                        Result = ex is not CustomException ? ex.StackTrace : null
                    });
                    context.ExceptionHandled = true;
                }
                return Task.CompletedTask;
            }
        }
    }
}
