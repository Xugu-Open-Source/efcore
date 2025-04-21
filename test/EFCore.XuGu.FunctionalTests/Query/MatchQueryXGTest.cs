using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.XuGu.Tests;
using Xunit;

namespace Microsoft.EntityFrameworkCore.XuGu.FunctionalTests.Query
{
    public class MatchQueryXGTest : MatchQueryXGTestBase<MatchQueryXGTest.MatchQueryXGFixture>
    {
        public MatchQueryXGTest(MatchQueryXGFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(3, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(5, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_keywords_separated()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First", XGMatchSearchMode.NaturalLanguage)|| EF.Functions.Match(herb.Name, "Second", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(6, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First') OR CONTAINS (h.Name,'Second')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_keywords_separated_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First, Second", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(8, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First, Second')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_multiple_keywords()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First", XGMatchSearchMode.NaturalLanguage) || EF.Functions.Match(herb.Name, "Herb", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(9, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First') OR CONTAINS (h.Name,'Herb')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_multiple_keywords_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First Herb", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(9, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First Herb')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_multiple_keywords_separated()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First", XGMatchSearchMode.NaturalLanguage) || EF.Functions.Match(herb.Name, "Second", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(6, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First') OR CONTAINS (h.Name,'Second')");
        }

        [ConditionalFact]
        public virtual void Match_in_natural_language_mode_multiple_keywords_separated_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First, Second", XGMatchSearchMode.NaturalLanguage));

            Assert.Equal(8, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First, Second')");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First%", XGMatchSearchMode.Boolean));

            Assert.Equal(3, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First%')");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First%", XGMatchSearchMode.Boolean));

            Assert.Equal(5, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`,'First%')");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_with_search_mode_parameter()
        {
            using var context = CreateContext();

            var searchMode = XGMatchSearchMode.Boolean;
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First%", searchMode));

            Assert.Equal(3, count);

            AssertSql(
                @":__searchMode_1='2'

SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE ((:__searchMode_1 = 0) AND CONTAINS (h.Name,'First%')) OR (((:__searchMode_1 = 1) AND CONTAINS (h.Name,'First%')) OR ((:__searchMode_1 = 2) AND CONTAINS (h.Name,'First%')))");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_with_search_mode_parameter_multiple_columns()
        {
            using var context = CreateContext();

            var searchMode = XGMatchSearchMode.Boolean;
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First%", searchMode));

            Assert.Equal(5, count);

            AssertSql(
                @":__searchMode_1='2'

SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE ((:__searchMode_1 = 0) AND CONTAINS (h.Name, `h`.`Garden`,'First%')) OR (((:__searchMode_1 = 1) AND CONTAINS (h.Name, `h`.`Garden`,'First%' WITH QUERY EXPANSION)) OR ((:__searchMode_1 = 2) AND CONTAINS (h.Name, `h`.`Garden`,'First%')))");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_keywords()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First%", XGMatchSearchMode.Boolean) || EF.Functions.Match(herb.Name,"Herb%", XGMatchSearchMode.Boolean));

            Assert.Equal(9, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First%') OR CONTAINS (h.Name,'Herb%')");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_keywords_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First% Herb*", XGMatchSearchMode.Boolean));

            Assert.Equal(9, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First% Herb*')");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_keyword_excluded()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "Herb%", XGMatchSearchMode.Boolean) && !EF.Functions.Match(herb.Name, "Second", XGMatchSearchMode.Boolean));

            Assert.Equal(6, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'Herb%') AND NOT (CONTAINS (h.Name,'Second'))");
        }

        [ConditionalFact]
        public virtual void Match_in_boolean_mode_keyword_excluded_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "Herb*", XGMatchSearchMode.Boolean) && !EF.Functions.Match(new[] { herb.Name, herb.Garden }, "Second", XGMatchSearchMode.Boolean));

            Assert.Equal(4, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'Herb* -Second')");
        }

        [ConditionalFact]
        public virtual void Match_with_query_expansion()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(herb.Name, "First", XGMatchSearchMode.NaturalLanguageWithQueryExpansion));

            Assert.Equal(3, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name,'First')");
        }

        [ConditionalFact]
        public virtual void Match_with_query_expansion_multiple_columns()
        {
            using var context = CreateContext();
            var count = context.Set<Herb>().Count(herb => EF.Functions.Match(new []{herb.Name, herb.Garden}, "First", XGMatchSearchMode.NaturalLanguageWithQueryExpansion));

            Assert.Equal(9, count);

            AssertSql(@"SELECT COUNT(*)
FROM `Herb` AS `h`
WHERE CONTAINS (h.Name, `h`.`Garden`, 'First' WITH QUERY EXPANSION)");
        }

        private void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class MatchQueryXGFixture : MatchQueryXGFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => XGTestStoreFactory.Instance;
        }
    }

    public abstract class MatchQueryXGTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : MatchQueryXGTestBase<TFixture>.MatchQueryXGFixtureBase, new()
    {
        protected MatchQueryXGTestBase(TFixture fixture)
            : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        protected virtual DbContext CreateContext() => Fixture.CreateContext();

        public abstract class MatchQueryXGFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
        {
            protected override string StoreName { get; } = "MatchQueryTest";
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<Herb>(
                    herb =>
                    {
                        herb.HasData(MatchQueryData.CreateHerbs());
                        herb.HasKey(h => h.Id);
                        herb.HasIndex(h => new {h.Name}).IsFullText();
                        herb.HasIndex(h => new {h.Name, h.Garden}).IsFullText();

                        // We force a case-insensitive collation here, because there exists a bug, where XuGu and MariaDB will handle
                        // FULLTEXT searches for `..._bin` collations incorrectly.
                        herb.Property(h => h.Name);
                        herb.Property(h => h.Garden);
                    });
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                return base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());
            }

            public override PoolableDbContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                return context;
            }

            public Func<DbContext> GetContextCreator()
                => CreateContext;

            public ISetSource GetExpectedData()
                => new MatchQueryData();

            public IReadOnlyDictionary<Type, object> EntitySorters
                => new Dictionary<Type, Func<object, object>> { { typeof(Herb), e => ((Herb)e)?.Id }, }.ToDictionary(e => e.Key,
                    e => (object)e.Value);

            public IReadOnlyDictionary<Type, object> EntityAsserters
                => new Dictionary<Type, Action<object, object>>
                {
                    {
                        typeof(Herb), (e, a) =>
                        {
                            Assert.Equal(e == null, a == null);

                            if (a != null)
                            {
                                var ee = (Herb)e;
                                var aa = (Herb)a;

                                Assert.Equal(ee.Id, aa.Id);
                                Assert.Equal(ee.Name, aa.Name);
                            }
                        }
                    },
                }.ToDictionary(e => e.Key, e => (object)e.Value);
        }

        public class MatchQueryData : ISetSource
        {
            private readonly IReadOnlyList<Herb> _herbs;

            public MatchQueryData()
            {
                _herbs = CreateHerbs();
            }

            public IQueryable<TEntity> Set<TEntity>() where TEntity : class
            {
                if (typeof(TEntity) == typeof(Herb))
                {
                    return (IQueryable<TEntity>)_herbs.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            public static IReadOnlyList<Herb> CreateHerbs()
            {
                return new List<Herb>
                {
                    new Herb {Id = 1, Name = "First Herb Name 1", Garden = "Garden spot"},
                    new Herb {Id = 2, Name = "First Herb Name 2", Garden = "First Farmer's Market"},
                    new Herb {Id = 3, Name = "First Herb Name 3", Garden = "Family's Second Farm"},
                    new Herb {Id = 4, Name = "Second Herb Name 1", Garden = "Garden spot"},
                    new Herb {Id = 5, Name = "Second Herb Name 2", Garden = "First Farmer's Market"},
                    new Herb {Id = 6, Name = "Second Herb Name 3", Garden = "Family's Second Farm"},
                    new Herb {Id = 7, Name = "Third Herb Name 1", Garden = "Garden spot"},
                    new Herb {Id = 8, Name = "Third Herb Name 2", Garden = "First Farmer's Market"},
                    new Herb {Id = 9, Name = "Third Herb Name 3", Garden = "Family's Second Farm"}
                };
            }
        }

        public class Herb
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Garden { get; set; }
        }
    }
}
