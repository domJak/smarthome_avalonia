using Xunit;
using SmartHome.ViewModels;

public class BulbViewModelTests
{
    [Fact]
    public void Toggle_ShouldChangeBulbState()
    {
        // Arrange
        var bulb = new BulbViewModel("test bulb");
        var initialState = bulb.IsOn;

        // Act
        bulb.ToggleCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialState, bulb.IsOn);
    }
}
