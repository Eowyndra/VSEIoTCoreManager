using Xunit;
using Moq;
using VSEIoTCoreServer.Services;
using System;
using VSEIoTCoreServer.ViewModels;

namespace VSEIoTCoreServer.UnitTest
{
    public class DeviceConfigurationViewModelTest
    {
        [Fact]
        public void ViewModelTest()
        {
            // Arrange
            int Id = 22;
            string VseType = "VSE002";
            string VseIpAddress = "192.168.0.1";
            int VsePort = 4711;
            int IoTCorePort = 4712;

            var deviceConfigurationViewModel = new DeviceConfigurationViewModel 
            { 
                Id = Id, 
                VseType = VseType, 
                VseIpAddress = VseIpAddress, 
                VsePort = VsePort, 
                IoTCorePort = IoTCorePort 
            };


            // Assert
            Assert.Equal(Id, deviceConfigurationViewModel.Id);
            Assert.Equal(VseType, deviceConfigurationViewModel.VseType);
            Assert.Equal(VseIpAddress, deviceConfigurationViewModel.VseIpAddress);
            Assert.Equal(VsePort, deviceConfigurationViewModel.VsePort);
            Assert.Equal(IoTCorePort, deviceConfigurationViewModel.IoTCorePort);
        }
    }
}