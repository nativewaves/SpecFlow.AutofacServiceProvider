Feature: DependencyInjectionPlugin
	As a developer I want to verify
	that the DependencyInjectionPlugin
	allows me to use Microsoft.Extensions.DependencyInjection

Scenario: Test service injection
	Then verify that TestService is correctly injected

Scenario: Feature and Scenario context can be injected
 Then the IFeatureContext is correctly injected
 And the FeatureContext is correctly injected
 And the IScenarioContext is correctly injected
 And the ScenarioContext is correctly injected
