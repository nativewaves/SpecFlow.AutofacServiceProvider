Feature: LifetimeScopeAndContextDisposal
    Checks that the autofac lifetime scope from the feature and scenario
    container are disposed and cleaning up all scoped/transient instances

Scenario: Assert context is disposed correctly
    Given I have a context with number 7
