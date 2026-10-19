using AgilePredict.Controllers;
using AgilePredict.Models.DTOs.SprintRisk;
using AgilePredict.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgilePredict.Tests.Controllers
{
    public class SprintRiskBotControllerTests
    {
        [Fact]
        public async Task GetReport_DelegatesToOrchestratorAndReturnsOk()
        {
            var orchestratorMock = new Mock<ISprintRiskBotOrchestrator>();
            var expectedReport = new SprintRiskReport { HistoricalDailyCapacity = 4.2 };
            orchestratorMock
                .Setup(o => o.GenerateReportAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedReport);

            var controller = new SprintRiskBotController(orchestratorMock.Object);

            var result = await controller.GetReport(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expectedReport, okResult.Value);
        }
    }
}
