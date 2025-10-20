Feature: IReqnrollOutputHelper
    Issue #75: ITestOutputHelper unavailable when using Reqnroll.xUnit
    https://github.com/solidtoken/Reqnroll.DependencyInjection/issues/75

Scenario: Assert IReqnrollOutputHelper Is Available
    The When part here is purely for displaying the message using IReqnrollOutputHelper
    When a message is output using IReqnrollOutputHelper
    Then verify that IReqnrollOutputHelper is correctly injected
