/* ===================================================================================
 * Classes to help using multiple devices on I2C bus
 * ===================================================================================
 * Inspired by a sample posted on http://forums.netduino.com (Netduino Forums) by
 * Jeremy (http://forums.netduino.com/index.php?/topic/2545-hmc5883l-magnetometer-netduino-code/)
 * And modified to fit indeed multiple devices on the I2C bus by by Zakie Mashiah
 * Small additions by Thomas D. Kryger
 * ===================================================================================
 * You may use, copy, borrow, modify or do anything you like with this code. Crediting
 * the author will be appreciated.
 * ===================================================================================
 */

using System;
using Microsoft.SPOT.Hardware;

namespace AbstractI2CDevice
{

    /// <summary>
    /// This class is used to have a single bus interface on the system as typically is the case on microprocessors.
    /// For some reason Microsoft chose to call the bus I2Device which is wrong as that class represent the bus really
    /// and not a single device on it.
    /// For simplicity we hold one I2Device.Configuration that will help programmers build classes to have interface 
    /// with single device on the bus with no hassle. If only a single device exist on the bus then program will be
    /// calling the 'Execute' function without having the configuration sent on every call.
    /// </summary>
    public static class I2CBus
    {
        private static object myLock = new object();

        public enum I2CBusSpeed : int
        {
            ClockRate100 = 100,
            ClockRate400 = 400  //ClockRate KiloHertz
        }

        static I2CDevice.Configuration currentConfig;
        static I2CDevice theBus;

        public static void SetConfig(ushort address, int clockRate)
        {
            lock (myLock)
            {
                currentConfig = new I2CDevice.Configuration(address, clockRate); //ClockRate KiloHertz
                if (theBus == null) // good time to initialize the bus
                    theBus = new I2CDevice(currentConfig);
            }
        }

        public static void SetConfig(I2CDevice.Configuration config)
        {
            lock (myLock)
            {
                currentConfig = config;
                if (theBus == null) // good time to initialize the bus
                    theBus = new I2CDevice(currentConfig);
            }
        }

        /// <summary>
        /// Executes a transaction by scheduling the transfer of the data involved.
        /// </summary>
        /// <param name="xAction">The object that contains the transaction data.</param>
        /// <param name="timeout">The amount of time the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes of data transferred in the transaction.</returns>
        public static void Execute(I2CDevice.I2CTransaction[] xAction, int timeout)
        {
            lock (myLock)
            {
                theBus.Config = currentConfig;
                theBus.Execute(xAction, timeout);
            }

        }

        /// <summary>
        /// Executes a transaction by scheduling the transfer of the data involved.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="xAction">The object that contains the transaction data.</param>
        /// <param name="timeout">The amount of time the system will wait before resuming execution of the transaction.</param>
        /// <returns>The number of bytes of data transferred in the transaction.</returns>
        public static void Execute(I2CDevice.Configuration config, I2CDevice.I2CTransaction[] xAction, int timeout)
        {
            lock (myLock)
            {
                theBus.Config = config;
                theBus.Execute(xAction, timeout);
            }
        }
    }



    /// <summary>
    /// This class helps abstract multiple devices on the I2C bus, so that every class representing a device should inherit this class
    /// and just implement the two abstract methods 'Connected' and 'DeviceIdentifiier' which are failry straight forward on most devices
    /// Also this class offers the option to read 16 bit values (short) and not only bytes array.
    /// </summary>
    public abstract class AbstractI2CDevice
    {
        protected I2CDevice.Configuration myConfig;
        int Timeout;

        public AbstractI2CDevice(int address, int clockRate, int timeout) // : base(new Configuration(address, clockRate)) 
        {
            myConfig = new I2CDevice.Configuration((ushort)address, clockRate);
            Timeout = timeout;
            I2CBus.SetConfig(myConfig);
        }

        /// <summary>
        /// </summary>Read any number of consecutive Registers
        /// <param name="addressToReadFrom"></param> Start at this address. 
        /// <param name="responseLength"></param> Response length is the number of Registers to read. If not specified, only one Register will be read.
        /// <returns></returns>
        public byte[] Read(byte addressToReadFrom, int responseLength = 1)
        {
            var buffer = new byte[responseLength];
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] { addressToReadFrom }),
                I2CDevice.CreateReadTransaction(buffer)
            };
            I2CBus.Execute(myConfig, transaction, Timeout);
            return buffer;
        }
        public ushort Read16(I2CDevice.I2CTransaction[] xAction)
        {
            I2CBus.Execute(myConfig, xAction, Timeout);
            return (ushort)(xAction[1].Buffer[0] << 8 | xAction[1].Buffer[1]);
        }


        public byte[] Read(I2CDevice.I2CTransaction[] xAction, int index = 1)
        {
            I2CBus.Execute(myConfig, xAction, Timeout);
            return xAction[index].Buffer;
        }

        //public byte[] Read1(byte[] addressToReadFrom, byte[] data)
        //{
        //    xAction[0] = I2CDevice.CreateWriteTransaction(addressToReadFrom);
        //    xAction[1] = I2CDevice.CreateReadTransaction(data);

        //    I2CBus.Execute(myConfig, xAction, Timeout);
        //    return data;
        //}
        //public ushort Read16(byte[] addressToReadFrom)
        //{
        //    xAction[0] = I2CDevice.CreateWriteTransaction(addressToReadFrom);
        //    xAction[1] = I2CDevice.CreateReadTransaction(readBuffer16);

        //    I2CBus.Execute(myConfig, xAction, Timeout);
        //    return (ushort)(readBuffer16[0] << 8 | readBuffer16[1]);
        //}

        public byte[] Read(int responseLength)
        {
            byte[] buffer = new byte[responseLength];
            I2CDevice.I2CTransaction[] transaction = null;
            transaction = new I2CDevice.I2CTransaction[] { I2CDevice.CreateReadTransaction(buffer) };
            I2CBus.Execute(myConfig, transaction, Timeout);
            return buffer;
        }

        public byte Read(byte reg)
        {
            //byte[] singleCommand = new byte[1];
            //singleCommand[0] = (byte)reg;

            //I2CDevice.I2CTransaction[] transaction;
            //transaction = new I2CDevice.I2CTransaction[]
            //{
            //    I2CDevice.CreateWriteTransaction(new byte[] { (byte)reg }),
            //    I2CDevice.CreateReadTransaction(_readBuffer)
            //};
            //I2CBus.Execute(myConfig, transaction, Timeout);
            //return _readBuffer[0];

            var buffer = new byte[1];
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
             {
                 I2CDevice.CreateWriteTransaction(new byte[] { reg }),
                 I2CDevice.CreateReadTransaction(buffer)
             };

            I2CBus.Execute(myConfig, transaction, Timeout);
            return buffer[0];
        }
        public ushort Read16(byte reg)
        {
            byte[] singleCommand = new byte[1];
            byte[] readBuffer16 = new byte[2];
            singleCommand[0] = (byte)reg;

            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] { (byte)reg }),
                I2CDevice.CreateReadTransaction(readBuffer16)
            };
            I2CBus.Execute(myConfig, transaction, Timeout);
            return (ushort)(readBuffer16[0] << 8 | readBuffer16[1]);
        }

        /// <summary>
        /// Reads 16 bit value from two registers on the I2C device
        /// </summary>
        /// <param name="addrMSB"></param>
        /// <param name="addrLSB"></param>
        /// <returns></returns>
        public short ReadShort(byte addrMSB, byte addrLSB)
        {
            short result;
            byte startAddr = 0;
            bool highFirst = false;
            byte[] data;

            // See if the addresses are continous and what order
            if ((addrLSB + 1) == addrMSB)
            {
                startAddr = addrLSB;
                highFirst = false;
            }
            else
                if ((addrMSB + 1) == addrLSB)
            {
                startAddr = addrMSB;
                highFirst = true;
            }

            // If they are continous then read 2 bytes from the bus
            if (startAddr != 0)
            {
                data = Read(startAddr, 2);

                if (highFirst)
                    result = (Int16)(data[0] << 8 | data[1]);
                else
                    result = (Int16)(data[1] << 8 | data[0]);
            }
            else
            {
                // Read one byte at a time
                byte lowV, highV;

                // lowV = Read(addrLSB)[0];
                //highV = Read(addrMSB)[0];
                // result = (Int16)(highV << 8 | lowV);
                result = 0;
            }

            return result;
        }

        /// <summary>
        /// Write one Byte to a Register
        /// </summary>
        /// <param name="addressToWriteTo"></param>
        /// <param name="valueToWrite"></param>
        public void Write(byte addressToWriteTo, byte valueToWrite)
        {
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] { addressToWriteTo, valueToWrite })
            };
            I2CBus.Execute(myConfig, transaction, Timeout);
        }

        public void Write(byte valueToWrite)
        {
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(new byte[] {valueToWrite })
            };
            I2CBus.Execute(myConfig, transaction, Timeout);
        }

        public void Write(I2CDevice.I2CTransaction[] xAction)
        {
            I2CBus.Execute(myConfig, xAction, Timeout);
        }

        public void WriteBytes(byte[] bytesToWrite)
        {
            I2CDevice.I2CTransaction[] transaction;
            transaction = new I2CDevice.I2CTransaction[]
            {
                I2CDevice.CreateWriteTransaction(bytesToWrite)
            };
            I2CBus.Execute(myConfig, transaction, Timeout);
        }





        public void Write(I2CDevice.I2CTransaction[] xActions, int timeout)
        {
            I2CBus.SetConfig(myConfig);
            I2CBus.Execute(xActions, timeout);
        }


        public abstract bool Connected();

        public abstract byte[] DeviceIdentifier();
    }

}
