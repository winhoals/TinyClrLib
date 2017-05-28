﻿using System.Diagnostics;
using System.Threading;
using BlepClick;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Pins;

partial class Program
{
    private BlepModule _blepModule;
    const byte TriggerStatePipeId = 1;
    const byte RedDotSightPowerPipeId = 2;

    const byte Timeout = 15;
    const byte Interval = 64;

    private GpioPin _trigger;
    private GpioPin _led;

    private readonly byte[][] SetupData = new byte[][]
    {
        new byte[] {0x00,0x00,0x03,0x02,0x42,0x07,},
        new byte[] {0x10,0x00,0x00,0x00,0x00,0x00,0x02,0x00,0x02,0x00,0x02,0x01,0x01,0x00,0x00,0x06,0x00,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,},
        new byte[] {0x10,0x1c,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x10,0x00,0x00,0x00,0x10,0x03,0x90,0x01,0xff,},
        new byte[] {0x10,0x38,0xff,0xff,0x02,0x58,0x00,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,},
        new byte[] {0x10,0x54,0x01,0x00,},
        new byte[] {0x20,0x00,0x04,0x04,0x02,0x02,0x00,0x01,0x28,0x00,0x01,0x00,0x18,0x04,0x04,0x05,0x05,0x00,0x02,0x28,0x03,0x01,0x02,0x03,0x00,0x00,0x2a,0x04,0x04,0x14,},
        new byte[] {0x20,0x1c,0x0f,0x00,0x03,0x2a,0x00,0x01,0x4c,0x61,0x73,0x65,0x72,0x54,0x61,0x67,0x20,0x47,0x75,0x6e,0x20,0x30,0x31,0x00,0x00,0x00,0x00,0x00,0x04,0x04,},
        new byte[] {0x20,0x38,0x05,0x05,0x00,0x04,0x28,0x03,0x01,0x02,0x05,0x00,0x01,0x2a,0x06,0x04,0x03,0x02,0x00,0x05,0x2a,0x01,0x01,0xc0,0x03,0x04,0x04,0x05,0x05,0x00,},
        new byte[] {0x20,0x54,0x06,0x28,0x03,0x01,0x02,0x07,0x00,0x04,0x2a,0x06,0x04,0x09,0x08,0x00,0x07,0x2a,0x04,0x01,0x06,0x00,0x06,0x00,0x00,0x00,0xff,0xff,0x04,0x04,},
        new byte[] {0x20,0x70,0x02,0x02,0x00,0x08,0x28,0x00,0x01,0x01,0x18,0x04,0x04,0x10,0x10,0x00,0x09,0x28,0x00,0x01,0x2d,0x2c,0xea,0xde,0x18,0xb5,0x1a,0xb2,0xb6,0x44,},
        new byte[] {0x20,0x8c,0xa8,0x36,0x00,0x10,0xfa,0xf0,0x04,0x04,0x13,0x13,0x00,0x0a,0x28,0x03,0x01,0x12,0x0b,0x00,0x2d,0x2c,0xea,0xde,0x18,0xb5,0x1a,0xb2,0xb6,0x44,},
        new byte[] {0x20,0xa8,0xa8,0x36,0x01,0x10,0xfa,0xf0,0x16,0x0c,0x02,0x01,0x00,0x0b,0x10,0x01,0x02,0x00,0x46,0x34,0x03,0x02,0x00,0x0c,0x29,0x02,0x01,0x00,0x00,0x04,},
        new byte[] {0x20,0xc4,0x04,0x10,0x10,0x00,0x0d,0x28,0x00,0x01,0x2d,0x2c,0xea,0xde,0x18,0xb5,0x1a,0xb2,0xb6,0x44,0xa8,0x36,0x00,0x20,0xfa,0xf0,0x04,0x04,0x13,0x13,},
        new byte[] {0x20,0xe0,0x00,0x0e,0x28,0x03,0x01,0x0a,0x0f,0x00,0x2d,0x2c,0xea,0xde,0x18,0xb5,0x1a,0xb2,0xb6,0x44,0xa8,0x36,0x01,0x20,0xfa,0xf0,0x46,0x3c,0x02,0x01,},
        new byte[] {0x20,0xfc,0x00,0x0f,0x20,0x01,0x02,0x00,0x00,},
        new byte[] {0x40,0x00,0x10,0x01,0x02,0x00,0x02,0x04,0x00,0x0b,0x00,0x0c,0x20,0x01,0x02,0x04,0x00,0x04,0x00,0x0f,0x00,0x00,},
        new byte[] {0x50,0x00,0x2d,0x2c,0xea,0xde,0x18,0xb5,0x1a,0xb2,0xb6,0x44,0xa8,0x36,0x00,0x00,0xfa,0xf0,},
        new byte[] {0x60,0x00,0x00,0x00,0x00,0x00,0x00,0x00,},
        new byte[] {0xf0,0x00,0x03,0x09,0x4a,}
    };

    public void BrainPadSetup()
    {
        //Put your setup code here. It runs once when the BrainPad starts up.
        _blepModule = new BlepModule(
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Rst,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Cs,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Int,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.An,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Miso,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Mosi,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Sck,
            GHIElectronics.TinyCLR.Pins.BrainPad.GpioPin.Pwm
            );
        _trigger = GpioController.GetDefault().OpenPin(GHIElectronics.TinyCLR.Pins.G30.GpioPin.PA15);
        _trigger.SetDriveMode(GpioPinDriveMode.InputPullUp);

        _led = GpioController.GetDefault().OpenPin(GHIElectronics.TinyCLR.Pins.G30.GpioPin.PB9);
        _led.SetDriveMode(GpioPinDriveMode.Output);
        _led.Write(GpioPinValue.Low);

        _blepModule.AciEventReceived += BlepModule_AciEventReceived;
        _blepModule.DataReceived += BlepModule_DataReceived;
        _blepModule.Setup(SetupData);



        //blepModule.OnCommandResponseEvent += BlepModule_OnCommandResponseEvent;
        //blepModule.OnDeviceStartedEvent += BlepModuleOnOnDeviceStartedEvent;
        //byte[] buff = new byte[] { 0x02, 0x02, 0xa7 };
        //_reqn.Write(GpioPinValue.Low);
        //WaitForNrfReady(GpioPinValue.Low);
        //spi.Write(buff);
        //_reqn.Write(GpioPinValue.High);
        //WaitForNrfReady(GpioPinValue.High);
        //Thread.Sleep(1000);
        //blepModule.SendGetDeviceVersionCommand();
        triggerState = _trigger.Read() == GpioPinValue.High;
        _blepModule.AwaitBond(Timeout, Interval);

    }

    private void BlepModule_DataReceived(BlepClick.Events.DataReceivedEvent dataReceivedEvent)
    {
        throw new System.NotImplementedException();
    }

    private void BlepModule_AciEventReceived(BlepClick.Events.AciEvent aciEvent)
    {
        throw new System.NotImplementedException();
    }

    private void BlepModule_OnCommandResponseEvent(int length, byte eventCode, byte commandOpCode, byte status, byte[] data)
    {
        Debug.WriteLine("New event _OnCommandResponseEvent: 0x" + eventCode.ToString("X") + " , OP code: 0x" + commandOpCode.ToString("X") + " , Status: 0x" + status);
        for (int i = 0; i < data.Length; i++)
        {
            Debug.WriteLine(i.ToString("X") + ": 0x" + data[i].ToString("X"));
        }
    }

    private void BlepModuleOnOnDeviceStartedEvent(int length, byte eventCode, byte operatingMode, byte hwError, byte dataCreditAvailable)
    {
        Debug.WriteLine("New event OnDeviceStartedEvent: " + eventCode.ToString("X"));
        //        throw new NotImplementedException();
    }

    private bool triggerState;

    public void BrainPadLoop()
    {
        //Put your program code here. It runs repeatedly after the BrainPad starts up.
        _blepModule.ProcessEvents();
        NotifyButtonState(_trigger, TriggerStatePipeId, ref triggerState);
    }

    private void NotifyButtonState(GpioPin trigger, byte triggerStatePipeId, ref bool state)
    {
        var currentState = trigger.Read() == GpioPinValue.High;
        if (currentState != state)
        {
            _blepModule.SendData(triggerStatePipeId, (byte) (currentState ? 0x00 : 0x01));
            state = currentState;
            Thread.Sleep(10);
        }
    }
}
