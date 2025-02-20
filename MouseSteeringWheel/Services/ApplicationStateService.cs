using System.Threading;

namespace MouseSteeringWheel.Services
{
    /// <summary>
    /// 应用程序状态管理服务（线程安全）
    /// </summary>
    public class ApplicationStateService
    {
        private int _isPaused = 0; // 0=运行中，1=已暂停

        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePauseState()
        {
            Interlocked.Exchange(ref _isPaused, IsPaused ? 0 : 1);
        }

        /// <summary>
        /// 当前是否处于暂停状态
        /// </summary>
        public bool IsPaused => Interlocked.CompareExchange(ref _isPaused, 0, 0) == 1;
    }
}
