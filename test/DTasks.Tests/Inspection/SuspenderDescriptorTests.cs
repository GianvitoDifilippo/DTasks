﻿using System.Reflection;
using static DTasks.Inspection.InspectionConstants;

namespace DTasks.Inspection;

public partial class SuspenderDescriptorTests
{
    [Theory]
    [InlineData(typeof(StructSuspender<>), typeof(StructDeconstructor), false)]
    [InlineData(typeof(ClassSuspender<>), typeof(ClassDeconstructor), false)]
    [InlineData(typeof(ByRefStructSuspender<>), typeof(StructDeconstructor), true)]
    [InlineData(typeof(ByRefClassSuspender<>), typeof(ClassDeconstructor), true)]
    public void TryCreate_ShouldCreateDescriptor_ForAllSuspenders(Type suspenderType, Type parameterType, bool byRef)
    {
        // Arrange

        // Act
        bool result = SuspenderDescriptor.TryCreate(suspenderType, out SuspenderDescriptor? sut);

        // Assert
        result.Should().BeTrue();
        sut.Should().NotBeNull();
        sut!.DelegateType.Should().Be(suspenderType);
        sut.DeconstructorParameter.ParameterType.Should().Be(byRef ? parameterType.MakeByRefType() : parameterType);
    }

    [Theory]
    [InlineData(typeof(SuspenderWithoutHandleField<>))]
    [InlineData(typeof(SuspenderWithoutHandleAwaiter<>))]
    public void TryCreate_ShouldNotCreateDescriptor_ForInvalidSuspenders(Type suspenderType)
    {
        // Arrange

        // Act
        bool result = SuspenderDescriptor.TryCreate(suspenderType, out SuspenderDescriptor? sut);

        // Assert
        result.Should().BeFalse();
        sut.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldCreateDescriptor_ForMinimalDeconstructor()
    {
        // Arrange
        Type deconstructorType = typeof(ClassDeconstructor);
        MethodInfo handleFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), Type.MakeGenericMethodParameter(0)]);
        MethodInfo handleAwaiterMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleAwaiter,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string)]);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeTrue();
        sut.Should().NotBeNull();
        sut!.Type.Should().Be(deconstructorType);
        sut.HandleStateMethod.Should().BeSameAs(handleFieldMethod.MakeGenericMethod(typeof(int)));
        sut.HandleAwaiterMethod.Should().BeSameAs(handleAwaiterMethod);
        sut.GetHandleFieldMethod(typeof(string)).Should().BeSameAs(handleFieldMethod.MakeGenericMethod(typeof(string)));
    }

    [Fact]
    public void Create_ShouldCreateDescriptor_ForDeconstructorWithHandleStateMethod()
    {
        // Arrange
        Type deconstructorType = typeof(DeconstructorWithHandleStateMethod);
        MethodInfo handleStateMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleState,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), typeof(int)]);
        MethodInfo handleAwaiterMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleAwaiter,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string)]);
        MethodInfo handleFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), Type.MakeGenericMethodParameter(0)]);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeTrue();
        sut.Should().NotBeNull();
        sut!.Type.Should().Be(deconstructorType);
        sut.HandleStateMethod.Should().BeSameAs(handleStateMethod);
        sut.HandleAwaiterMethod.Should().BeSameAs(handleAwaiterMethod);
        sut.GetHandleFieldMethod(typeof(string)).Should().BeSameAs(handleFieldMethod.MakeGenericMethod(typeof(string)));
    }

    [Fact]
    public void Create_ShouldCreateDescriptor_ForDeconstructorWithSpecializedMethod()
    {
        // Arrange
        Type deconstructorType = typeof(DeconstructorWithSpecializedMethod);
        MethodInfo handleIntFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), typeof(int)]);
        MethodInfo handleAwaiterMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleAwaiter,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string)]);
        MethodInfo handleFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), Type.MakeGenericMethodParameter(0)]);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeTrue();
        sut.Should().NotBeNull();
        sut!.Type.Should().Be(deconstructorType);
        sut.HandleStateMethod.Should().BeSameAs(handleIntFieldMethod);
        sut.HandleAwaiterMethod.Should().BeSameAs(handleAwaiterMethod);
        sut.GetHandleFieldMethod(typeof(string)).Should().BeSameAs(handleFieldMethod.MakeGenericMethod(typeof(string)));
        sut.GetHandleFieldMethod(typeof(int)).Should().BeSameAs(handleIntFieldMethod);
    }

    [Fact]
    public void Create_ShouldCreateDescriptor_ForDeconstructorWithSpecializedMethodAndHandleStateMethod()
    {
        // Arrange
        Type deconstructorType = typeof(DeconstructorWithSpecializedMethodAndHandleStateMethod);
        MethodInfo handleStateMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleState,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), typeof(int)]);
        MethodInfo handleAwaiterMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleAwaiter,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string)]);
        MethodInfo handleFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), Type.MakeGenericMethodParameter(0)]);
        MethodInfo handleIntFieldMethod = deconstructorType.GetRequiredMethod(
            name: MethodNames.HandleField,
            bindingAttr: BindingFlags.Instance | BindingFlags.Public,
            parameterTypes: [typeof(string), typeof(int)]);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeTrue();
        sut.Should().NotBeNull();
        sut!.Type.Should().Be(deconstructorType);
        sut.HandleStateMethod.Should().BeSameAs(handleStateMethod);
        sut.HandleAwaiterMethod.Should().BeSameAs(handleAwaiterMethod);
        sut.GetHandleFieldMethod(typeof(string)).Should().BeSameAs(handleFieldMethod.MakeGenericMethod(typeof(string)));
        sut.GetHandleFieldMethod(typeof(int)).Should().BeSameAs(handleIntFieldMethod);
    }

    [Fact]
    public void Create_ShouldNotCreateDescriptor_ForDeconstructorWithoutHandleField()
    {
        // Arrange
        Type deconstructorType = typeof(DeconstructorWithoutHandleField);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeFalse();
        sut.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldNotCreateDescriptor_ForDeconstructorWithoutHandleAwaiter()
    {
        // Arrange
        Type deconstructorType = typeof(DeconstructorWithoutHandleAwaiter);

        // Act
        bool result = DeconstructorDescriptor.TryCreate(deconstructorType, out DeconstructorDescriptor? sut);

        // Assert
        result.Should().BeFalse();
        sut.Should().BeNull();
    }
}
