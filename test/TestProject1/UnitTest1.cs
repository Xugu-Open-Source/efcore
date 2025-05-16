// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Design.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using XuguClient;
using Xunit;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseXG(
                "IP=127.0.0.1;DB=LMSEFCore70test3;User=SYSDBA;PWD=SYSDBA;Port=5138;AUTO_COMMIT=off;CHAR_SET=GBK;").Options;

            using (var context = new MyDbContext(options))
            {
                // Ensure database is deleted to start fresh
                context.Database.EnsureDeleted();

                // Act
                context.Database.Migrate();

                // Assert
                var migrator = context.GetService<IMigrator>();
                var appliedMigrations = context.Database.GetAppliedMigrations();

                // Check if the latest migration is applied
                Assert.Contains("LatestMigrationName", appliedMigrations);
            }
        }

        private IReverseEngineerScaffolder CreateScaffolder()
        {
            var services = new ServiceCollection();

            // 1. 注入 EFCore.Design 的核心设计时服务，注册 ReverseEngineerScaffolder
            new EntityFrameworkDesignServicesBuilder(services)
                .TryAddCoreServices();

            // 2. 使用 DesignTimeServicesBuilder 扫描 Relational 程序集，
            //    将数据库模型工厂、代码生成工厂等“关系型”反向工程服务注册进来
            var reporter = new OperationReporter(new OperationReportHandler(m => { /* 可选：写日志 */ }));
            var startupAssembly = Assembly.GetExecutingAssembly();                        // 测试项目自身
            var relationalAssembly = typeof(IRelationalTypeMappingSource).Assembly;       // Microsoft.EntityFrameworkCore.Relational.dll
            var builder = new DesignTimeServicesBuilder(
                args: Array.Empty<string>(),
                startupAssembly: startupAssembly,
                assembly: relationalAssembly,
                reporter: reporter);

            // **注意：这里调用的是实例方法 ConfigureDesignTimeServices(IServiceCollection)**
            builder.ConfigureDesignTimeServices(services);

            // 3. 最后注入你自己 XG Provider 的设计时服务
            new XGDesignTimeServices()
                .ConfigureDesignTimeServices(services);

            // 构建并返回 IReverseEngineerScaffolder
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IReverseEngineerScaffolder>();
        }
    }
}
