//// Copyright (c) Pomelo Foundation. All rights reserved.
//// Licensed under the MIT. See LICENSE in the project root for license information.

//using System;
//using System.Data.Common;
//using System.Reflection;
//using System.Text;
//using JetBrains.Annotations;
//using Microsoft.EntityFrameworkCore.Storage;
//using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//using XuguClient;
//using NetTopologySuite;
//using NetTopologySuite.Geometries;
//using NetTopologySuite.IO;
//using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
//using Microsoft.EntityFrameworkCore.XuGu.Storage.ValueConversion.Internal;

//namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
//{
//    /// <summary>
//    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//    ///     any release. You should only use it directly in your code with extreme caution and knowing that
//    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//    /// </summary>
//    public class XGGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, XGGeometry>
//        where TGeometry : Geometry
//    {
//        private readonly IXGOptions _options;

//        // ReSharper disable once StaticMemberInGenericType
//        private static readonly MethodInfo _getXGGeometry
//            = typeof(XGDataReader).GetRuntimeMethod(nameof(XGDataReader.GetXGGeometry), new[] { typeof(int) });

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        [UsedImplicitly]
//        public XGGeometryTypeMapping(NtsGeometryServices geometryServices, string storeType, IXGOptions options)
//            : base(
//                new GeometryValueConverter<TGeometry>(geometryServices),
//                storeType)
//        {
//            _options = options;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected XGGeometryTypeMapping(
//            RelationalTypeMappingParameters parameters,
//            ValueConverter<TGeometry, XGGeometry> converter,
//            IXGOptions options)
//            : base(parameters, converter)
//        {
//            _options = options;
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
//            => new XGGeometryTypeMapping<TGeometry>(parameters, SpatialConverter, _options);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override string GenerateNonNullSqlLiteral(object value)
//        {
//            // We are skipping the whole "ST_GeomFromText()" call here, because XuGu 8 switches the (lon, lat)
//            // parameter order depending on what SRID is being used.
//            // Just supplying the value as a WKB hex string should work for all database servers and versions.
//            var xgGeometry = (XGGeometry)SpatialConverter.ConvertToProvider(value);
//            var hexString = BitConverter.ToString(xgGeometry.Value)
//                .Replace("-", string.Empty);

//            return $"X'{hexString}'";
//        }

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        public override MethodInfo GetDataReaderMethod()
//            => _getXGGeometry;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override string AsText(object value)
//            => ((Geometry)value).AsText();

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override int GetSrid(object value)
//            => ((Geometry)value).SRID;

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override Type WktReaderType
//            => typeof(WKTReader);

//        /// <summary>
//        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
//        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
//        ///     any release. You should only use it directly in your code with extreme caution and knowing that
//        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
//        /// </summary>
//        protected override void ConfigureParameter(DbParameter parameter)
//        {
//            base.ConfigureParameter(parameter);

//            var xgParameter = (XGParameters)parameter;
//            xgParameter.XGDbType = XGDbType.Geometry;
//        }

//        protected virtual bool CheckEmptyValue(Geometry geometry)
//        {
//            // Only `GeometryCollection.Empty` is currently supported by XuGu and MariaDB.
//            if (geometry == Point.Empty ||
//                geometry == LineString.Empty ||
//                geometry == Polygon.Empty ||
//                geometry == MultiPoint.Empty ||
//                geometry == MultiLineString.Empty ||
//                geometry == MultiPolygon.Empty)
//            {
//                throw new InvalidOperationException($@"An empty spatial geometry value has been used for a type of ""{geometry.GetType()}"". The only empty value currently supported by XuGu and MariaDB is ""GeometryCollection.Empty"".");
//            }

//            return geometry == GeometryCollection.Empty;
//        }
//    }
//}
