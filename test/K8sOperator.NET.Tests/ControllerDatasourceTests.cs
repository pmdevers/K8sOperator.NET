namespace K8sOperator.NET.Tests;

using AwesomeAssertions;
using k8s;
using K8sOperator.NET.Tests.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

public class ControllerDatasourceTests
{
    private readonly List<object> _metadata;
    private readonly ControllerDatasource _controllerDatasource;
    private readonly ITestOutputHelper _testOutput;

    public ControllerDatasourceTests(ITestOutputHelper testOutput)
    {
        _metadata = [];
        _controllerDatasource = new ControllerDatasource(_metadata);
        _testOutput = testOutput;
    }

    [Fact]
    public void AddController_Should_AddControllerEntryWithConventions()
    {
        // Arrange
        var controllerType = typeof(MyController);

        // Act
        var conventionBuilder = _controllerDatasource.AddController(controllerType);

        // Assert
        conventionBuilder.Should().NotBeNull();
        _controllerDatasource.Metadata.Should().BeSameAs(_metadata);
    }

    [Fact]
    public void AddController_Should_AddConventionsToEntry()
    {
        // Arrange
        var controllerType = typeof(MyController);
        var kubernetes = Substitute.For<IKubernetes>();
        var provide = new ServiceCollection()
            .AddLogging(x =>
            {
                x.ClearProviders();
                x.AddTestOutput(_testOutput);
            })
            .AddSingleton(kubernetes)
            .BuildServiceProvider();

        // Act
        var conventionBuilder = _controllerDatasource.AddController(controllerType);

        // Adding a sample convention
        conventionBuilder.Add(builder => { /* Convention logic */ });

        var watcher = _controllerDatasource.GetWatchers(provide).FirstOrDefault();

        watcher.Should().NotBeNull();
        watcher?.Controller.Should().BeOfType<MyController>();
    }

    [Fact]
    public void GetWatchers_Should_ReturnEventWatchers_WithAppliedConventions()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<IKubernetes>());
        services.AddLogging(x =>
        {
            x.ClearProviders();
            x.AddTestOutput(_testOutput);
        });
        // Arrange
        var serviceProvider = services.BuildServiceProvider();
        var controllerType = typeof(MyController);

        _controllerDatasource.AddController(controllerType);

        // Act
        var watchers = _controllerDatasource.GetWatchers(serviceProvider);

        watchers.Should().HaveCount(1);
    }

    [Fact]
    public void AddAfterProcessBuildConventionCollection_Should_ThrowIfModifiedAfterBuild()
    {
        // Arrange
        var collection = new ControllerDatasource.AddAfterProcessBuildConventionCollection
        {
            IsReadOnly = true
        };

        // Act
        Action act = () => collection.Add(builder => { });

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*can not be modified after build*");
    }
}

