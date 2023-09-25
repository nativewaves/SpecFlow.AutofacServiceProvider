﻿namespace NativeWaves.SpecFlow.AutofacServiceProvider.Tests.Support
{
    public interface ICalculator
    {
        void Enter(int operand);
        void Add();
        void Multiply();
        int Result { get; }
        int Size { get; }
    }
}
