using System;
using vJoyInterfaceWrap;
using MouseSteeringWheel.Properties;

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
            _vJoyDeviceId = (uint)Settings.Default.vJoyDeviceId;

            // 初始化vJoy
            InitializevJoy();
        }

        /// <summary>
        /// 初始化 vJoy 并进行运行前检测
        /// </summary>
        public bool InitializevJoy()
        {
            _joyStick = new vJoy();

            // vJoy 运行前检测
            if (!CheckvJoyDriverEnabled()) return false;
            if (!CheckDriverMatch()) return false;
            if (!CheckDeviceStatus(_vJoyDeviceId)) return false;
            if (!AcquireVJoyDevice(_vJoyDeviceId)) return false;
            if (!CheckVJoyDeviceProperties(_vJoyDeviceId)) return false;

            // 重置 X 轴数值，使方向盘居中
            SetJoystickX(16383);

            // 重置 Y 轴数值，使刹车油门归零
            SetJoystickY(16383);

            iReport = new vJoy.JoystickState() { bDevice = (byte)_vJoyDeviceId };

            // 设备初始化成功
            //Console.WriteLine("Vendor: {0}\nProduct :{1}\nVersion Number:{2}\n",
            //    _joyStick.GetvJoyManufacturerString(),
            //    _joyStick.GetvJoyProductString(),
            //    _joyStick.GetvJoySerialNumberString());
            return true;
        }

        #region 检测 vJoy 状态

        /// <summary>
        /// 检测驱动是否成功初始化
        /// </summary>
        public bool CheckvJoyDriverEnabled()
        {
            if (!_joyStick.vJoyEnabled())
            {
                _messageBoxService.ShowMessage(Resources.InitializationFailedContent, Resources.InitializationFailedTitle);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检测用户安装的驱动版本是否与本软件使用的 SDK 匹配
        /// </summary>
        public bool CheckDriverMatch()
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (!_joyStick.DriverMatch(ref DllVer, ref DrvVer))
            {
                _messageBoxService.ShowMessage(Resources.vJoyVersionMismatchContent, Resources.vJoyVersionMismatchTitle);
                return false;
            }
            //Console.WriteLine("Version of Driver Matches DLL Version ({0:X})\n", DllVer);
            return true;
        }

        // 
        /// <summary>
        /// 检测 vJoy 设备状态
        /// </summary>
        /// <param name="id">用户指定的 vJoy 设备 ID</param>
        public bool CheckDeviceStatus(uint id)
        {
            status = _joyStick.GetVJDStatus(id);

            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    //Console.WriteLine($"vJoy Device {id} is already owned");
                    break;
                case VjdStat.VJD_STAT_FREE:
                    //Console.WriteLine($"vJoy Device {id} is free");
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    _messageBoxService.ShowMessage(Resources.vJoyDeviceBusyContent, Resources.vJoyDeviceBusyTitle);
                    return false;
                case VjdStat.VJD_STAT_MISS:
                    _messageBoxService.ShowMessage(Resources.vJoyDeviceNotFoundContent, Resources.vJoyDeviceNotFoundTitle);
                    return false;
                default:
                    _messageBoxService.ShowMessage(Resources.vJoyErrorContent, Resources.vJoyErrorTitle);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 检测是否占有 vJoy 设备，如果未占有则获取设备
        /// </summary>
        public bool AcquireVJoyDevice(uint id)
        {
            status = _joyStick.GetVJDStatus(id);
            // 获取目标设备
            if ((status == VjdStat.VJD_STAT_OWN) ||
                    ((status == VjdStat.VJD_STAT_FREE) && (!_joyStick.AcquireVJD(id))))
                _messageBoxService.ShowMessage(Resources.vJoyDeviceObtainFailedContent, Resources.vJoyDeviceObtainFailedTitle);
            //else
            //    Console.WriteLine($"Acquired: vJoy device number {id}.");

            return true;
        }

        /// <summary>
        /// 检测 XY 轴摇杆是否激活
        /// </summary>
        public bool CheckVJoyDeviceProperties(uint id)
        {
            if (Settings.Default.XAxisEnable && !_joyStick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_X))
                _messageBoxService.ShowMessage(Resources.vJoyXAxisNotDetectedContent, Resources.vJoyXAxisNotDetectedTitle);
            if (Settings.Default.YAxisEnable && !_joyStick.GetVJDAxisExist(id, HID_USAGES.HID_USAGE_Y))
                _messageBoxService.ShowMessage(Resources.vJoyYAxisNotDetectedContent, Resources.vJoyYAxisNotDetectedTitle);

            return true;
        }

        #endregion


        #region 鼠标移动相关

        /// <summary>
        /// 获取vJoy设备的摇杆X轴状态
        /// </summary>
        public int GetJoystickX()
        {
            //获取 vJoy 的所有位置信息
            _joyStick.GetPosition(_vJoyDeviceId, ref iReport);
            //返回X轴坐标
            return iReport.AxisX;
        }

        /// <summary>
        /// 设置 vJoy 设备的摇杆 X 轴位置
        /// </summary>
        public void SetJoystickX(int val)
        {
            _joyStick.SetAxis(val, _vJoyDeviceId, HID_USAGES.HID_USAGE_X);
        }

        /// <summary>
        /// 获取 vJoy 设备的摇杆 Y 轴状态
        /// </summary>
        public int GetJoystickY()
        {
            //获取 vJoy 的所有位置信息
            _joyStick.GetPosition(_vJoyDeviceId, ref iReport);
            //返回 Y 轴坐标
            return iReport.AxisY;
        }

        /// <summary>
        /// 设置 vJoy 设备的摇杆 Y 轴状态
        /// </summary>
        public void SetJoystickY(int val)
        {
            _joyStick.SetAxis(val, _vJoyDeviceId, HID_USAGES.HID_USAGE_Y);
        }

        /// <summary>
        /// 重置摇杆位置
        /// </summary>
        public void ResetJoystickPos()
        {
            CheckDeviceStatus(_vJoyDeviceId);
            SetJoystickX(16383);
            SetJoystickY(16383);
        }

        #endregion


        #region 按键相关

        /// <summary>
        /// 映射键盘按键到vJoy按键
        /// </summary>
        public void MapKeyToButton(int buttonId)
        {
            _joyStick.SetBtn(true, _vJoyDeviceId, (uint)buttonId);
        }

        public void ResetButtonStatus(int buttonId)
        {
            _joyStick.SetBtn(false, _vJoyDeviceId, (uint)buttonId);
        }

        #endregion


        #region 解除占用

        /// <summary>
        /// 实现 IDisposable 接口
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
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
                        //Console.WriteLine("Device Relinquished!");
                    }
                }
                // 释放非托管资源
                _disposed = true;

            }
        }

        ~vJoyService()
        {
            Dispose(false);
        }

        #endregion
    }
}
