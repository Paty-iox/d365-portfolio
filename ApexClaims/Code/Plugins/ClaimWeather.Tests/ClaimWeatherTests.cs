using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace ApexClaims.Plugins.Tests
{
    public class ClaimWeatherTests
    {
        private const string ClaimEntityName = "new_claim";
        private const string IncidentLatitudeField = "new_incidentlatitude";
        private const string IncidentLongitudeField = "new_incidentlongitude";
        private const string IncidentDateField = "new_incidentdate";
        private const string WeatherConditionsField = "new_weatherconditions";

        private Mock<IPluginExecutionContext> _contextMock;
        private Mock<IOrganizationServiceFactory> _serviceFactoryMock;
        private Mock<IOrganizationService> _serviceMock;
        private Mock<ITracingService> _traceMock;
        private Mock<IServiceProvider> _serviceProviderMock;

        public ClaimWeatherTests()
        {
            _contextMock = new Mock<IPluginExecutionContext>();
            _serviceFactoryMock = new Mock<IOrganizationServiceFactory>();
            _serviceMock = new Mock<IOrganizationService>();
            _traceMock = new Mock<ITracingService>();
            _serviceProviderMock = new Mock<IServiceProvider>();

            _serviceFactoryMock
                .Setup(f => f.CreateOrganizationService(It.IsAny<Guid?>()))
                .Returns(_serviceMock.Object);

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IPluginExecutionContext)))
                .Returns(_contextMock.Object);
            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IOrganizationServiceFactory)))
                .Returns(_serviceFactoryMock.Object);
            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(ITracingService)))
                .Returns(_traceMock.Object);
        }

        [Fact]
        public void Execute_WrongEntity_SkipsProcessing()
        {
            // Arrange
            _contextMock.Setup(c => c.PrimaryEntityName).Returns("contact");
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(new ParameterCollection());

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Wrong entity")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_WrongMessage_SkipsProcessing()
        {
            // Arrange
            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Delete");
            _contextMock.Setup(c => c.InputParameters).Returns(new ParameterCollection());

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Wrong message")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_NoTargetEntity_SkipsProcessing()
        {
            // Arrange
            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(new ParameterCollection());

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("No target")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_UpdateWithoutRelevantFieldChange_SkipsProcessing()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target["new_someotherfield"] = "value";

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Update");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("No relevant fields")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_MissingCoordinates_SkipsWeatherLookup()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target[IncidentLatitudeField] = null;

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            // Mock Retrieve to return claim without coordinates
            var retrievedClaim = new Entity(ClaimEntityName, claimId);
            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, claimId, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Missing coordinates")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_MissingIncidentDate_SkipsWeatherLookup()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target[IncidentLatitudeField] = -33.8688m;

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            // Mock Retrieve to return claim with coordinates but no date
            var retrievedClaim = new Entity(ClaimEntityName, claimId);
            retrievedClaim[IncidentLatitudeField] = -33.8688m;
            retrievedClaim[IncidentLongitudeField] = 151.2093m;
            // No incident date

            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, claimId, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Missing incident date")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_FutureIncidentDate_SkipsWeatherLookup()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target[IncidentDateField] = DateTime.UtcNow.AddDays(5);

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            // Mock Retrieve
            var retrievedClaim = new Entity(ClaimEntityName, claimId);
            retrievedClaim[IncidentLatitudeField] = -33.8688m;
            retrievedClaim[IncidentLongitudeField] = 151.2093m;
            retrievedClaim[IncidentDateField] = DateTime.UtcNow.AddDays(5);

            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, claimId, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("future")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_InvalidCoordinateRange_SkipsWeatherLookup()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target[IncidentLatitudeField] = 999m; // Invalid

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            // Mock Retrieve
            var retrievedClaim = new Entity(ClaimEntityName, claimId);
            retrievedClaim[IncidentLatitudeField] = 999m; // Invalid latitude
            retrievedClaim[IncidentLongitudeField] = 151.2093m;
            retrievedClaim[IncidentDateField] = DateTime.UtcNow.AddDays(-1);

            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, claimId, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Invalid coordinate")), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public void Execute_MissingEnvironmentVariables_SkipsWeatherLookup()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var target = new Entity(ClaimEntityName, claimId);
            target[IncidentLatitudeField] = -33.8688m;

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns("Create");
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            // Mock Retrieve
            var retrievedClaim = new Entity(ClaimEntityName, claimId);
            retrievedClaim[IncidentLatitudeField] = -33.8688m;
            retrievedClaim[IncidentLongitudeField] = 151.2093m;
            retrievedClaim[IncidentDateField] = DateTime.UtcNow.AddDays(-1);

            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, claimId, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            // Mock empty environment variables
            _serviceMock
                .Setup(s => s.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection());

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert
            _serviceMock.Verify(s => s.Update(It.IsAny<Entity>()), Times.Never);
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("must be configured")), It.IsAny<object[]>()), Times.Once);
        }

        [Theory]
        [InlineData("Create")]
        [InlineData("Update")]
        public void Execute_ValidMessages_Accepted(string messageName)
        {
            // Arrange
            var target = new Entity(ClaimEntityName, Guid.NewGuid());
            target[IncidentLatitudeField] = -33.8688m;

            var inputParams = new ParameterCollection { { "Target", target } };

            _contextMock.Setup(c => c.PrimaryEntityName).Returns(ClaimEntityName);
            _contextMock.Setup(c => c.MessageName).Returns(messageName);
            _contextMock.Setup(c => c.InputParameters).Returns(inputParams);
            _contextMock.Setup(c => c.UserId).Returns(Guid.NewGuid());

            var retrievedClaim = new Entity(ClaimEntityName, target.Id);
            _serviceMock
                .Setup(s => s.Retrieve(ClaimEntityName, target.Id, It.IsAny<ColumnSet>()))
                .Returns(retrievedClaim);

            var plugin = new ClaimWeather();

            // Act
            plugin.Execute(_serviceProviderMock.Object);

            // Assert - should not reject due to wrong message
            _traceMock.Verify(t => t.Trace(It.Is<string>(msg => msg.Contains("Wrong message")), It.IsAny<object[]>()), Times.Never);
        }
    }
}
