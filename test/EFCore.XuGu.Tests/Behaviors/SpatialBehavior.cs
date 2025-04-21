// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Tests.TestUtilities.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.XuGu.Behaviors
{
    [SupportedServerVersionLessThanCondition(nameof(ServerVersionSupport.SpatialSetSridFunction))]
    public class SpatialBehavior : RawSqlTestWithFixture<SpatialBehavior.SpatialBehaviorFixture>
    {
        public SpatialBehavior(SpatialBehaviorFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
        }

        [Fact]
        public void NetTopologySuite_uses_x_longitude_and_y_latitude_order()
        {
            var iceCreamShops = Context.Set<Model.IceCreamShop>().ToList();

            Assert.All(iceCreamShops, s => Assert.Equal(13.3777041, s.Location.X));
            Assert.All(iceCreamShops, s => Assert.Equal(52.5162746, s.Location.Y));
        }

        public static class Model
        {
            public class IceCreamShop
            {
                public int IceCreamShopId { get; set; }
                public string Name { get; set; }
                public Point Location { get; set; }
            }

            public class SpatialBehaviorContext : ContextBase
            {
                public DbSet<IceCreamShop> IceCreamShops { get; set; }

                protected override void OnModelCreating(ModelBuilder modelBuilder)
                {
                    var srid = 4326;
                    var factory = NtsGeometryServices.Instance.CreateGeometryFactory(srid);

                    // Add a shop through NTS and Pomelo.
                    modelBuilder.Entity<IceCreamShop>(
                        entity =>
                        {
                            // This has no effect on XuGu < 8 and MariaDB, because XuGu < 8 and MariaDB do not support SRID constraints.
                            //
                            // entity.Property(e => e.Location)
                            //     .HasSpatialReferenceSystem(srid);

                            entity.HasData(
                                new IceCreamShop
                                {
                                    IceCreamShopId = 1,
                                    Name = "Cold & Sweet",

                                    // Brandenburg Gate, Berlin, Germany:
                                    // lon=13.3777041, lat=52.5162746
                                    // or @52.516377,13.3776013 as Google Maps outputs them in @lat,lon order)
                                    Location = factory.CreatePoint(new Coordinate( /* lon */ x: 13.3777041, /* lat */ y: 52.5162746))
                                });
                        });
                }
            }
        }

        public class SpatialBehaviorFixture : XGTestFixtureBase<Model.SpatialBehaviorContext>
        {
            // Add a shop directly through SQL.
            protected override string SetupDatabaseScript
                => @"
insert into `IceCreamShops` (`IceCreamShopId`, `Name`, `Location`) values (2, 'Cold & Sweet', ST_GeomFromText('POINT(13.3777041 52.5162746)', 4326)); -- lon lat
insert into `IceCreamShops` (`IceCreamShopId`, `Name`, `Location`) values (3, 'Cold & Sweet', ST_GeomFromText('POINT(13.3777041 52.5162746)', 0)); -- lon lat
insert into `IceCreamShops` (`IceCreamShopId`, `Name`, `Location`) values (4, 'Cold & Sweet', ST_GeomFromText('POINT(13.3777041 52.5162746)')); -- lon lat
insert into `IceCreamShops` (`IceCreamShopId`, `Name`, `Location`) values (5, 'Cold & Sweet', Point(13.3777041, 52.5162746)); -- lon lat";

            public override DbContext CreateDefaultDbContext()
                => CreateContext(
                    xgOptions: builder => builder.UseNetTopologySuite(),
                    serviceCollection: collection => collection.AddEntityFrameworkXGNetTopologySuite());
        }
    }
}
