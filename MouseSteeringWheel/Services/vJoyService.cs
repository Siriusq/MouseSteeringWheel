﻿using System;
using vJoyInterfaceWrap;

namespace MouseSteeringWheel.Services
{
    public class vJoyService : IDisposable
    {
        private readonly MessageBoxService _messageBoxService;
        public vJoy _joyStick;
        private vJoy.JoystickState iReport;
        private VjdStat status;
        public uint _vJoyDeviceId;
        private bool _disposed = false;

        public vJoyService(MessageBoxService messageBoxService)
        {
            _messageBoxService = messageBoxService;
            //Todo: 将此数值加入到设置中，然后读取
            _vJoyDeviceId = 1;

            // 初始化vJoy
            if (!InitializevJoy())
            {
                Console.WriteLine("Initialize Failed!");
            }
        }

        // 初始化vJoy并返回是否成功
        public bool InitializevJoy()
        {
            _joyStick = new vJoy();

            // vJoy运行前检测
            if (!CheckvJoyDriverEnabled()) return false;
            if (!CheckDriverMatch()) return false;
            if (!CheckDeviceStatus(_vJoyDeviceId)) return false;
            if (!AcquireVJoyDevice(_vJoyDeviceId)) return false;
            if (!CheckVJoyDeviceProperties(_vJoyDeviceId)) return false;

            // 重置X数值，使方向盘居中
            SetJoystickX(16383);

            // 重置Y数值，使刹车油门归零
            SetJoystickY(16383);

            iReport = new vJoy.JoystickState() { bDevice = (byte)_vJoyDeviceId };

            // 设备初始化成功
            Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
                _joyStick.GetvJoyManufacturerString(),
                _joyStick.GetvJoyProductString(),
                _joyStick.GetvJoySerialNumberString());
            return true;
        }

        #region Check vJoy Status

        // 检测驱动是否成功初始化
        public bool CheckvJoyDriverEnabled()
        {
            if (!_joyStick.vJoyEnabled())
            {
                _messageBoxService.ShowMessage("vJoy 驱动未安装或未正确初始化，请确保系统安装了 vJoy 驱动程序。", "初始化失败");
                return false;
            }
            return true;
        }

        // 检测用户安装的驱动版本是否与本软件使用的SDK匹配
        public bool CheckDriverMatch()
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (!_joyStick.DriverMatch(ref DllVer, ref DrvVer))
            {
                _messageBoxService.ShowMessage("vJoy 版本不匹配。", "版本不匹配");
                return false;
            }
            Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            return true;
        }

        // 检测 vJoy 设备状态
        public bool CheckDeviceStatus(uint id)
        {
            status = _joyStick.GetVJDStatus(id);

            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    Console.WriteLine($"vJoy Device {id} is already owned");
                    break;
                case VjdStat.VJD_STAT_FREE:
                    Console.WriteLine($"vJoy Device {id} is free");
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    _messageBoxService.ShowMessage($"vJoy Device {id} is busy", "vJoy设备忙");
                    return false;
                case VjdStat.VJD_STAT_MISS:
                    _messageBoxService.ShowMessage($"vJoy Device {id} is missing", "未找到指定设备");
                    return false;
                default:
                    _messageBoxService.ShowMessage($"vJoy Device {id} general error", "设备错误");
                    return false;
            }

            return true;
        }

        // 检测是否占有 vJoy 设备，如果未占有则获取设备
        public bool AcquireVJoyDevice(uint id)
        {
            //	Write access to vJoy Device - Basic            
            status = _joyStick.GetVJDStatus(id);
            // Acquire the target
            if ((status == VjdStat.VJD_STAT_OWN) ||
                    ((status == VjdStat.VJD_STAT_FREE) && (!_joyStick.AcquireVJD(id))))
                _messageBoxService.ShowMessage($"Failed to acquire vJoy device number {id}.", "无法获取设备");
            else
                Console.WriteLine($"Acquired: vJoy device number {id}.");

            return true;
        }

        // 检测摇杆是否激活
        public bool CheckVJoyDeviceProperties(uint id)
        {
            //	vJoy Device properties
            if (!_joyStick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X))
                _messageBoxService.ShowMessage($"X axis not detected.", "X轴摇杆未开启");
            else
                Console.WriteLine("X axis activated.");

            return true;
        }

        #endregion


        #region 鼠标移动相关
        // 获取vJoy设备的摇杆X轴状态
        public int GetJoystickX()
        {
            //获取vJoy的所有位置信息
            _joyStick.GetPosition(_vJoyDeviceId, ref iReport);
            //返回X轴坐标
            return iReport.AxisX;
        }


        // 设置vJoy设备的摇杆X轴状态
        public void SetJoystickX(int val)
        {
            _joyStick.SetAxis(val, _vJoyDeviceId, HID_USAGES.HID_USAGE_X);
        }

        // 获取vJoy设备的摇杆Y轴状态
        public int GetJoystickY()
        {
            //获取vJoy的所有位置信息
            _joyStick.GetPosition(_vJoyDeviceId, ref iReport);
            //返回X轴坐标
            return iReport.AxisY;
        }


        // 设置vJoy设备的摇杆Y轴状态
        public void SetJoystickY(int val)
        {
            _joyStick.SetAxis(val, _vJoyDeviceId, HID_USAGES.HID_USAGE_Y);
        }

        #endregion

        #region 按键相关
        // 映射键盘按键到vJoy按键
        public void MapKeyToButton(int key)
        {
            switch (key)
            {
                case 2: // 小键盘 2 A
                    _joyStick.SetBtn(true, _vJoyDeviceId, 1); // 按下 vJoy 按钮 1
                    break;
                case 4: // 小键盘 4 X
                    _joyStick.SetBtn(true, _vJoyDeviceId, 2); // 按下 vJoy 按钮 2
                    break;
                case 6: // 小键盘 6 B 
                    _joyStick.SetBtn(true, _vJoyDeviceId, 3); // 按下 vJoy 按钮 3
                    break;
                case 8: // 小键盘 8 Y
                    _joyStick.SetBtn(true, _vJoyDeviceId, 4); // 按下 vJoy 按钮 4
                    break;
                case 7: // 小键盘 7 LT
                    _joyStick.SetBtn(true, _vJoyDeviceId, 1); // 按下 vJoy 按钮 1
                    break;
                case 9: // 小键盘 9 RT
                    _joyStick.SetBtn(true, _vJoyDeviceId, 2); // 按下 vJoy 按钮 2
                    break;
                case 1: // 小键盘 1 LB 
                    _joyStick.SetBtn(true, _vJoyDeviceId, 3); // 按下 vJoy 按钮 3
                    break;
                case 3: // 小键盘 3 RB
                    _joyStick.SetBtn(true, _vJoyDeviceId, 4); // 按下 vJoy 按钮 4
                    break;
                default:
                    break;
            }
            Console.WriteLine(key);
        }

        #endregion



        #region 解除占用

        // 解除占用
        public void Relinquish(uint id)
        {
            status = _joyStick.GetVJDStatus(id);
            if (status == VjdStat.VJD_STAT_OWN)
            {
                _joyStick.RelinquishVJD(id);
            }
        }

        // 实现 IDisposable 接口
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // 释放资源
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 如果占用，则释放托管资源
                    if (_joyStick.GetVJDStatus(_vJoyDeviceId) == VjdStat.VJD_STAT_OWN)
                    {
                        _joyStick.RelinquishVJD(_vJoyDeviceId); // 释放设备
                    }
                }
                // 释放非托管资源
                _disposed = true;
                Console.WriteLine("Device Relinquished!");
            }
        }

        ~vJoyService()
        {
            Dispose(false);
        }

        #endregion
    }
}
