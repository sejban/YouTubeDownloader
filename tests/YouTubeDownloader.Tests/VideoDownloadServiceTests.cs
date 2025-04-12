using NUnit.Framework;
using Moq;
using YouTubeDownloader.Services;
using YouTubeDownloader.Models;

namespace YouTubeDownloader.Tests
{
    [TestFixture]
    public class VideoDownloadServiceTests
    {
        private VideoDownloadService _videoDownloadService;
        private Mock<IVideoDownloader> _mockVideoDownloader;

        [SetUp]
        public void Setup()
        {
            _mockVideoDownloader = new Mock<IVideoDownloader>();
            _videoDownloadService = new VideoDownloadService(_mockVideoDownloader.Object);
        }

        [Test]
        public void DownloadVideo_ValidUrl_ReturnsVideoInfo()
        {
            // Arrange
            var videoUrl = "https://www.youtube.com/watch?v=example";
            var expectedVideoInfo = new VideoInfo
            {
                Title = "Example Video",
                Url = videoUrl,
                Duration = 120
            };

            _mockVideoDownloader.Setup(v => v.Download(videoUrl)).Returns(expectedVideoInfo);

            // Act
            var result = _videoDownloadService.DownloadVideo(videoUrl);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedVideoInfo.Title, result.Title);
            Assert.AreEqual(expectedVideoInfo.Url, result.Url);
            Assert.AreEqual(expectedVideoInfo.Duration, result.Duration);
        }

        [Test]
        public void DownloadVideo_InvalidUrl_ThrowsException()
        {
            // Arrange
            var invalidUrl = "invalid_url";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _videoDownloadService.DownloadVideo(invalidUrl));
        }
    }
}