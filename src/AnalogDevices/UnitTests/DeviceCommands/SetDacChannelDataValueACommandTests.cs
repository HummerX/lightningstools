﻿using System;
using AnalogDevices.DeviceCommands;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace AnalogDevices.UnitTests.DeviceCommands
{
    [TestFixture]
    public class SetDacChannelDataValueACommandTests
    {
        [Test]
        public void ShouldThrowExceptionIfChannelAddressIsOutOfRange(
            [Values((int) (ChannelAddress.Dac0 - 1), (int) (ChannelAddress.Dac39 + 1))] ChannelAddress channelAddress)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => WhenSetDacChannelDataValueA(channelAddress, ushort.MaxValue));
        }

        [Test]
        public void ShouldClearInputRegisterSelectBit(
            [Values((byte) ControlRegisterBits.InputRegisterSelect, (byte) ControlRegisterBits.WritableBits,
                byte.MaxValue, byte.MinValue)] byte controlRegisterBits,
            [Range((int) ChannelAddress.Dac0, (int) ChannelAddress.Dac39)] ChannelAddress channelAddress,
            [Values(ushort.MinValue, 1, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9, 1 << 10,
                1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, ushort.MaxValue)] int dacChannelDataValueA)
        {
            GivenControlRegisterBits((ControlRegisterBits) controlRegisterBits);
            WhenSetDacChannelDataValueA(channelAddress, (ushort) dacChannelDataValueA);
            ThenInputRegisterSelectBitInControlRegisterBitsIsCleared();
        }

        [Test]
        public void ShouldSendSPI(
            [Values(DacPrecision.SixteenBit, DacPrecision.FourteenBit)] DacPrecision dacPrecision,
            [Range((int) ChannelAddress.Dac0, (int) ChannelAddress.Dac39)] ChannelAddress channelAddress,
            [Values(ushort.MinValue, 1, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9, 1 << 10,
                1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, ushort.MaxValue)] int dacChannelDataValueA)
        {
            GivenDACPrecision(dacPrecision);
            WhenSetDacChannelDataValueA(channelAddress, (ushort) dacChannelDataValueA);
            ThenSPICommandIsSent(DataForSPI(dacPrecision, channelAddress, (ushort) dacChannelDataValueA));
        }

        [Test]
        public void ShouldCacheX1ARegisterValues(
            [Values(DacPrecision.SixteenBit, DacPrecision.FourteenBit)] DacPrecision dacPrecision,
            [Range((int) ChannelAddress.Dac0, (int) ChannelAddress.Dac39)] ChannelAddress channelAddress,
            [Values(ushort.MinValue, 1, 1 << 1, 1 << 2, 1 << 3, 1 << 4, 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9, 1 << 10,
                1 << 11, 1 << 12, 1 << 13, 1 << 14, 1 << 15, ushort.MaxValue)] int dacChannelDataValueA)
        {
            GivenDACPrecision(dacPrecision);
            WhenSetDacChannelDataValueA(channelAddress, (ushort) dacChannelDataValueA);
            ThenX1ARegisterValueInDeviceStateCacheIsSetTo(
                (int) channelAddress - 8,
                (ushort) (dacChannelDataValueA &
                          (dacPrecision == DacPrecision.SixteenBit
                              ? (ushort) BasicMasks.SixteenBits
                              : (ushort) BasicMasks.HighFourteenBits))
            );
        }

        private void GivenDACPrecision(DacPrecision dacPrecision)
        {
            _deviceState.Precision = dacPrecision;
        }

        private void GivenControlRegisterBits(ControlRegisterBits controlRegisterBits)
        {
            _controlRegisterBits = controlRegisterBits & ControlRegisterBits.ReadableBits;
            Mock.Get(_fakeReadbackControlRegisterCommand)
                .Setup(x => x.ReadbackControlRegister())
                .Returns(_controlRegisterBits)
                .Verifiable();
        }

        private void WhenSetDacChannelDataValueA(ChannelAddress channelAddress, ushort dacChannelDataValueA)
        {
            _setDacChannelDataValueACommand.SetDacChannelDataValueA(channelAddress, dacChannelDataValueA);
        }

        private void ThenInputRegisterSelectBitInControlRegisterBitsIsCleared()
        {
            Mock.Get(_fakeWriteControlRegisterCommand)
                .Verify(x =>
                    x.WriteControlRegister(
                        It.IsAny<ControlRegisterBits>()
                    ), Times.Once);

            _controlRegisterBitsWritten
                .Should()
                .Be(
                    _controlRegisterBits &
                    ~ControlRegisterBits.InputRegisterSelect
                    & ControlRegisterBits.WritableBits
                );
        }

        private void ThenSPICommandIsSent(uint data)
        {
            Mock.Get(_fakeSendSPICommand)
                .Verify(x => x.SendSPI(data), Times.Once);
        }

        private void ThenX1ARegisterValueInDeviceStateCacheIsSetTo(int registerNum, ushort data)
        {
            _deviceState.X1ARegisters[registerNum].Should().Be(data);
        }

        private static uint DataForSPI(DacPrecision dacPrecision, ChannelAddress channelAddress,
            ushort dacChannelDataValueA)
        {
            return
                (uint) SerialInterfaceModeBits.WriteToDACInputDataRegisterX
                | (uint) (((byte) channelAddress & (byte) BasicMasks.SixBits) << 16)
                | (dacChannelDataValueA &
                   (dacPrecision == DacPrecision.SixteenBit
                       ? (uint) BasicMasks.SixteenBits
                       : (uint) BasicMasks.HighFourteenBits));
        }

        [SetUp]
        public void SetUp()
        {
            _fakeEvalBoard = Mock.Of<IDenseDacEvalBoard>();
            _deviceState = new DeviceState();
            Mock.Get(_fakeEvalBoard)
                .SetupGet(x => x.DeviceState)
                .Returns(_deviceState);

            _controlRegisterBits = ControlRegisterBits.ReadableBits;
            _controlRegisterBitsWritten = null;

            _fakeReadbackControlRegisterCommand = Mock.Of<IReadbackControlRegister>();

            _fakeWriteControlRegisterCommand = Mock.Of<IWriteControlRegister>();
            Mock.Get(_fakeWriteControlRegisterCommand)
                .Setup(x => x.WriteControlRegister(It.IsAny<ControlRegisterBits>()))
                .Callback<ControlRegisterBits>(a => _controlRegisterBitsWritten = a)
                .Verifiable();

            _fakeSendSPICommand = Mock.Of<ISendSPI>();
            Mock.Get(_fakeSendSPICommand)
                .Setup(x => x.SendSPI(It.IsAny<uint>()))
                .Verifiable();

            _setDacChannelDataValueACommand = new SetDacChannelDataValueACommand(
                _fakeEvalBoard, _fakeReadbackControlRegisterCommand,
                _fakeWriteControlRegisterCommand, _fakeSendSPICommand);
        }

        private IDenseDacEvalBoard _fakeEvalBoard;
        private DeviceState _deviceState;
        private IReadbackControlRegister _fakeReadbackControlRegisterCommand;
        private IWriteControlRegister _fakeWriteControlRegisterCommand;
        private ISendSPI _fakeSendSPICommand;
        private ISetDacChannelDataValueA _setDacChannelDataValueACommand;
        private ControlRegisterBits _controlRegisterBits;
        private ControlRegisterBits? _controlRegisterBitsWritten;
    }
}