using System;
using Microsoft.SPOT.Hardware;

namespace IngenuityMicro.Sensors
{
    public enum Register : byte
    {
        MCP9808_REG_CONFIG = 0x01,
        MCP9808_REG_UPPER_TEMP = 0x02,
        MCP9808_REG_LOWER_TEMP = 0x03,
        MCP9808_REG_CRIT_TEMP = 0x04,
        MCP9808_REG_AMBIENT_TEMP = 0x05,
        MCP9808_REG_MANUF_ID = 0x06,
        MCP9808_REG_DEVICE_ID = 0x07,
    }

    public enum Config : ushort
    {
        MCP9808_REG_CONFIG_SHUTDOWN = 0x0100,
        MCP9808_REG_CONFIG_CRITLOCKED = 0x0080,
        MCP9808_REG_CONFIG_WINLOCKED = 0x0040,
        MCP9808_REG_CONFIG_INTCLR = 0x0020,
        MCP9808_REG_CONFIG_ALERTSTAT = 0x0010,
        MCP9808_REG_CONFIG_ALERTCTRL = 0x0008,
        MCP9808_REG_CONFIG_ALERTSEL = 0x0004,
        MCP9808_REG_CONFIG_ALERTPOL = 0x0002,
        MCP9808_REG_CONFIG_ALERTMODE = 0x0001,
    }

    public class MCP9808 : AbstractI2CDevice.AbstractI2CDevice
    {
        private Single _temp;
        private ushort _result;
        private byte[] AmTemp = new byte[] { 0x05 };
        private I2CDevice.I2CTransaction[] xAction;
        byte[] readBuffer = new byte[2];
        public MCP9808(byte address = 0x18, int clockRate = 400, int timeout = 1000) : base(address, clockRate, timeout)
        {
            Init();
            xAction = new I2CDevice.I2CTransaction[2];
            xAction[0] = I2CDevice.CreateWriteTransaction(AmTemp);
            xAction[1] = I2CDevice.CreateReadTransaction(readBuffer);
        }

        private void Init()
        {
            if (Read16((byte)Register.MCP9808_REG_MANUF_ID) != 0x54)
                throw new Exception("Bad manufacturer ID");
            if (Read16((byte)Register.MCP9808_REG_DEVICE_ID) != 0x0400)
                throw new Exception("Bad device ID");
        }

        public float ReadTempInC()
        {
            _result = Read16(xAction);
            _temp = _result & 0x0FFF;
            _temp /= 16.0F;
            if ((_result & 0x1000) != 0)
                _temp -= 256;
            return _temp;
        }

        public override bool Connected()
        {
            if ((Read16((byte)Register.MCP9808_REG_MANUF_ID) == 0x0054) & (Read16((byte)Register.MCP9808_REG_DEVICE_ID) == 0x0400))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override byte[] DeviceIdentifier()
        {
            throw new Exception("Bad manufacturer ID");
            //return (Read16((byte)Register.MCP9808_REG_DEVICE_ID));
        }
    }
}
