using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using StrangePower.Data;
using StrangePower.Services;
using System.Net;
using System.Text.Json;

namespace StrangePower.Tests.Services
{
    [TestClass]
    public class TokenServiceTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory = null!;
        private Mock<ITokenRepository> _mockTokenRepository = null!;
        private Mock<IConfiguration> _mockConfiguration = null!;
        private TokenService _tokenService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockConfiguration = new Mock<IConfiguration>();

            _tokenService = new TokenService(_mockHttpClientFactory.Object, _mockTokenRepository.Object, _mockConfiguration.Object);
        }

        [TestMethod]
        public async Task GetAccessTokenAsync_ShouldReturnExistingToken()
        {
            // Arrange
            var existingToken = new Token { AccessToken = "existing_token", Expiry = DateTime.UtcNow.AddHours(1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(existingToken);

            // Act
            var token = await _tokenService.GetAccessTokenAsync();

            // Assert
            Assert.AreEqual("existing_token", token);
        }

        [TestMethod]
        public async Task GetAccessTokenAsync_ShouldReturnNewTokenWhenExpired()
        {
            // Arrange
            var expiredToken = new Token { AccessToken = "expired_token", Expiry = DateTime.UtcNow.AddHours(-1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(expiredToken);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Result\":\"new_access_token\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns("dummy_refresh_token");

            // Act
            var token = await _tokenService.GetAccessTokenAsync();

            // Assert
            Assert.AreEqual("new_access_token", token);
            _mockTokenRepository.Verify(r => r.UpdateTokenAsync(It.Is<Token>(t => t.AccessToken == "new_access_token")), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetAccessTokenAsync_ShouldThrowExceptionWhenNoRefreshToken()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns(string.Empty);

            // Act
            await _tokenService.GetAccessTokenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task GetAccessTokenAsync_ShouldThrowExceptionWhenTokenRetrievalFails()
        {
            // Arrange
            var expiredToken = new Token { AccessToken = "expired_token", Expiry = DateTime.UtcNow.AddHours(-1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(expiredToken);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns("dummy_refresh_token");

            // Act
            await _tokenService.GetAccessTokenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(JsonException))]
        public async Task GetAccessTokenAsync_ShouldThrowExceptionWhenFailedToRetrieveNewToken()
        {
            // Arrange
            var expiredToken = new Token { AccessToken = "expired_token", Expiry = DateTime.UtcNow.AddHours(-1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(expiredToken);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"InvalidResult\":\"invalid_access_token\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns("dummy_refresh_token");

            // Act
            await _tokenService.GetAccessTokenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetAccessTokenAsync_ShouldThrowExceptionWhenTokenJsonResultIsInvalid()
        {
            // Arrange
            var expiredToken = new Token { AccessToken = "expired_token", Expiry = DateTime.UtcNow.AddHours(-1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(expiredToken);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"Result\":\"\"}") // Empty Result to simulate invalid TokenJsonResult
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns("dummy_refresh_token");

            // Act
            await _tokenService.GetAccessTokenAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GetAccessTokenAsync_ShouldThrowExceptionWhenTokenJsonResultIsNull()
        {
            // Arrange
            var expiredToken = new Token { AccessToken = "expired_token", Expiry = DateTime.UtcNow.AddHours(-1) };
            _mockTokenRepository.Setup(r => r.GetTokenAsync()).ReturnsAsync(expiredToken);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("null") // Simulate null TokenJsonResult
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
            _mockConfiguration.Setup(c => c["Authentication:RefreshToken"]).Returns("dummy_refresh_token");

            // Act
            await _tokenService.GetAccessTokenAsync();
        }
    }
}