using System;
using System.Threading;
using System.Threading.Tasks;
using Ambev.DeveloperEvaluation.Messaging.Implementations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Messaging
{
    public class MessagePublisherTests
    {
        [Fact]
        public async Task PublishAsync_LogsInformationAndCompletes()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MessagePublisher>>();
            var publisher = new MessagePublisher(loggerMock.Object);

            // Act
            var task = publisher.PublishAsync("test-message", CancellationToken.None);
            await task;

            // Assert - verify the logger was called and the returned task is completed
            Assert.True(task.IsCompleted);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publish here on the true message broker")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void PublishAsync_ReturnsCompletedTask_Immediately()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MessagePublisher>>();
            var publisher = new MessagePublisher(loggerMock.Object);

            // Act
            var task = publisher.PublishAsync(123);

            // Assert - Task should already be completed (Task.CompletedTask)
            Assert.True(task.IsCompleted);
            Assert.False(task.IsCanceled);
            Assert.Null(task.Exception);
        }

        [Fact]
        public async Task PublishAsync_WithCancelledToken_StillCompletesAndLogs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MessagePublisher>>();
            var publisher = new MessagePublisher(loggerMock.Object);
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // cancelled token

            // Act
            var task = publisher.PublishAsync(new { Value = 123 }, cts.Token);
            await task;

            // Assert - method should complete normally and still log
            Assert.True(task.IsCompleted);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publish here on the true message broker")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_WithDifferentMessageTypes_AlwaysLogs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MessagePublisher>>();
            var publisher = new MessagePublisher(loggerMock.Object);

            // Act - string
            await publisher.PublishAsync("string-message");
            // Act - object
            await publisher.PublishAsync(new { Name = "obj" });
            // Act - value type
            await publisher.PublishAsync(42);

            // Assert - logger invoked for each call
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Publish here on the true message broker")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));
        }
    }
}