using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace coderush.Data.UnitTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="DbInitializer"/> class.
    /// </summary>
    [TestClass]
    public class DbInitializerTests
    {
        /// <summary>
        /// Tests that Initialize calls EnsureCreatedAsync, and when no users exist,
        /// it calls CreateDefaultSuperAdmin and InitAppData.
        /// </summary>
        [TestMethod]
        public async Task Initialize_WhenNoUsersExist_CallsAllInitializationMethods()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Strict, new object[] { null! });
            mockDatabase.Setup(d => d.EnsureCreatedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var mockDbSet = new Mock<DbSet<ApplicationUser>>();
            var data = Enumerable.Empty<ApplicationUser>().AsQueryable();

            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ApplicationUser>(data.Provider));
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IAsyncEnumerable<ApplicationUser>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ApplicationUser>(data.GetEnumerator()));

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockContext.Setup(c => c.ApplicationUser).Returns(mockDbSet.Object);

            var mockFunctional = new Mock<IFunctional>(MockBehavior.Strict);
            mockFunctional.Setup(f => f.CreateDefaultSuperAdmin()).Returns(Task.CompletedTask);
            mockFunctional.Setup(f => f.InitAppData()).Returns(Task.CompletedTask);

            // Act
            await DbInitializer.Initialize(mockContext.Object, mockFunctional.Object);

            // Assert
            mockDatabase.Verify(d => d.EnsureCreatedAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockFunctional.Verify(f => f.CreateDefaultSuperAdmin(), Times.Once);
            mockFunctional.Verify(f => f.InitAppData(), Times.Once);
        }

        /// <summary>
        /// Tests that Initialize calls EnsureCreatedAsync, but when users exist,
        /// it returns early without calling CreateDefaultSuperAdmin or InitAppData.
        /// </summary>
        [TestMethod]
        public async Task Initialize_WhenUsersExist_ReturnsEarlyWithoutCallingFunctionalMethods()
        {
            // Arrange
            var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Strict, new object[] { null! });
            mockDatabase.Setup(d => d.EnsureCreatedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var existingUser = new ApplicationUser();
            var mockDbSet = new Mock<DbSet<ApplicationUser>>();
            var userList = new[] { existingUser }.AsQueryable();

            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ApplicationUser>(userList.Provider));
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<ApplicationUser>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ApplicationUser>(userList.GetEnumerator()));

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockContext.Setup(c => c.ApplicationUser).Returns(mockDbSet.Object);

            var mockFunctional = new Mock<IFunctional>(MockBehavior.Strict);

            // Act
            await DbInitializer.Initialize(mockContext.Object, mockFunctional.Object);

            // Assert
            mockDatabase.Verify(d => d.EnsureCreatedAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockFunctional.Verify(f => f.CreateDefaultSuperAdmin(), Times.Never);
            mockFunctional.Verify(f => f.InitAppData(), Times.Never);
        }

        /// <summary>
        /// Tests that Initialize calls methods in the correct order:
        /// EnsureCreatedAsync, then AnyAsync check, then CreateDefaultSuperAdmin, then InitAppData.
        /// </summary>
        [TestMethod]
        public async Task Initialize_CallsMethodsInCorrectOrder()
        {
            // Arrange
            var callOrder = new System.Collections.Generic.List<string>();

            var mockDatabase = new Mock<DatabaseFacade>(MockBehavior.Strict, new object[] { null! });
            mockDatabase.Setup(d => d.EnsureCreatedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .Callback(() => callOrder.Add("EnsureCreatedAsync"));

            var data = Enumerable.Empty<ApplicationUser>().AsQueryable();
            var mockDbSet = new Mock<DbSet<ApplicationUser>>();

            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ApplicationUser>(data.Provider, () => callOrder.Add("AnyAsync")));
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(data.Expression);
            mockDbSet.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockDbSet.As<IAsyncEnumerable<ApplicationUser>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ApplicationUser>(data.GetEnumerator()));

            var mockContext = new Mock<ApplicationDbContext>(MockBehavior.Loose, new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);
            mockContext.Setup(c => c.ApplicationUser).Returns(mockDbSet.Object);

            var mockFunctional = new Mock<IFunctional>(MockBehavior.Strict);
            mockFunctional.Setup(f => f.CreateDefaultSuperAdmin())
                .Returns(Task.CompletedTask)
                .Callback(() => callOrder.Add("CreateDefaultSuperAdmin"));
            mockFunctional.Setup(f => f.InitAppData())
                .Returns(Task.CompletedTask)
                .Callback(() => callOrder.Add("InitAppData"));

            // Act
            await DbInitializer.Initialize(mockContext.Object, mockFunctional.Object);

            // Assert
            Assert.AreEqual(4, callOrder.Count);
            Assert.AreEqual("EnsureCreatedAsync", callOrder[0]);
            Assert.AreEqual("AnyAsync", callOrder[1]);
            Assert.AreEqual("CreateDefaultSuperAdmin", callOrder[2]);
            Assert.AreEqual("InitAppData", callOrder[3]);
        }

        /// <summary>
        /// Helper class to support async enumeration for testing DbSet.AnyAsync.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly System.Collections.Generic.IEnumerator<T> _inner;

            public TestAsyncEnumerator(System.Collections.Generic.IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return default;
            }
        }

        /// <summary>
        /// Helper class to support async query provider for testing EF Core async operations like AnyAsync.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            private readonly Action _onExecuteAsync;

            internal TestAsyncQueryProvider(IQueryProvider inner, Action onExecuteAsync = null)
            {
                _inner = inner;
                _onExecuteAsync = onExecuteAsync;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new EnumerableQuery<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new EnumerableQuery<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                _onExecuteAsync?.Invoke();
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })!
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new object[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult })!;
            }
        }
    }
}