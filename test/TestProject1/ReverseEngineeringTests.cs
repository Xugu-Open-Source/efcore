// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1
{
    public class ReverseEngineeringTests
    {
        private IServiceProvider BuildDesignTimeServiceProvider()
        {
            var services = new ServiceCollection();
            // 注册 EF Core 设计时服务，包括 IReverseEngineerScaffolder
            services.AddEntityFrameworkDesignTimeServices();
            // 注册数据库提供者的设计时服务
            var xgServices = new Microsoft.EntityFrameworkCore.XuGu.Design.Internal.XGDesignTimeServices();
            xgServices.ConfigureDesignTimeServices(services);
            return services.BuildServiceProvider();
        }



        private readonly IServiceProvider _serviceProvider;
        private readonly string _outputDir;

        public ReverseEngineeringTests()
        {
            _serviceProvider = BuildDesignTimeServiceProvider();
            // 使用临时目录存放生成结果
            _outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_outputDir);
        }

        [Fact]
        public void ScaffoldModel_Generates_Cs_Files()
        {
            // Arrange
            var scaffolder = _serviceProvider.GetRequiredService<IReverseEngineerScaffolder>();
            string connectionString = "ip=127.0.0.1;db=LMSEFCore70TEST37;user=SYSDBA;pwd=SYSDBA;port=5138;auto_commit=off;char_set=utf8";
            var dbOptions = new DatabaseModelFactoryOptions(
                tables: Enumerable.Empty<string>(),
                schemas: Enumerable.Empty<string>());
            var revOptions = new ModelReverseEngineerOptions
            {
                UseDatabaseNames = true   // 保留原表名与列名
            };
            var codeOptions = new ModelCodeGenerationOptions
            {
                ContextName = "MyDbContext",
                ContextDir = _outputDir,
                ModelNamespace = "Models",
                ContextNamespace = "Data",
                //OutputDir = _outputDir,
                UseDataAnnotations = false
            };

            // Act
            var scaffoldedModel = scaffolder.ScaffoldModel(
                connectionString,
                dbOptions,
                revOptions,
                codeOptions);
            scaffolder.Save(
                scaffoldedModel,
                outputDir: _outputDir,
                overwriteFiles: true);

            // Assert
            var files = Directory.GetFiles(_outputDir, "*.cs", SearchOption.AllDirectories);
            Assert.Contains(files, f => f.EndsWith("MyDbContext.cs"));
            Assert.Contains(files, f => Path.GetFileName(f).EndsWith(".cs"));
        }

        public void Dispose()
        {
            // 清理临时目录
            if (Directory.Exists(_outputDir))
            {
                Directory.Delete(_outputDir, recursive: true);
            }
        }
    }
}
