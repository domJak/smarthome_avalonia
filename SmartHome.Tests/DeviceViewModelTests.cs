using Xunit;
using SmartHome.ViewModels;

public class DeviceViewModelTests
{
    [Fact]
    public void ToggleDevice_ShouldChangeState()
    {
        // Arrange
        var vm = new DeviceViewModel
        {
            IsOn = false
        };

        // Act
        vm.ToggleDevice();

        // Assert
        Assert.True(vm.IsOn);
    }
}
