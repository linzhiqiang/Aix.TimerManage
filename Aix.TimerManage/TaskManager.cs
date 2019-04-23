using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    /// <summary>
    /// 实例级别的任务管理，不受其他任务的影响，可自行启动关闭
    /// </summary>
    public class TaskManager
    {
        private List<ITimerTask> Tasks = new List<ITimerTask>();

        /// <summary>
        /// 注册定时任务
        /// </summary>
        /// <param name="job">要执行的任务</param>
        /// <param name="dueTime">开始延迟时间 毫秒</param>
        /// <param name="period">执行间隔时间 毫秒</param>
        public void RegisterTimerTask(IMyJob job, int dueTime, int period)
        {
            Add(new TimerTask(job, dueTime, period));
        }

        /// <summary>
        /// 注册定时任务  crontab表达式
        /// </summary>
        /// <param name="job"></param>
        /// <param name="expression">crontab表达式  * * * * *（分 时 日 月 周）</param>
        public void RegisterCrontabTask(IMyJob job, string expression)
        {
            Add(new CrontabTask(expression, job));
        }

        /// <summary>
        /// 注册自旋任务 就是while(true){  } 自己控制延迟时间
        /// </summary>
        /// <param name="job"></param>
        public void RegisterSpinTimerTassk(IMyJob job)
        {
            RegisterSpinTimerTassk(job, 1);
        }

        /// <summary>
        /// 注册自旋任务 就是while(true){  } 自己控制延迟时间
        /// </summary>
        /// <param name="job"></param>
        /// <param name="threadNumber"></param>
        public void RegisterSpinTimerTassk(IMyJob job, int threadNumber)
        {
            for (int i = 0; i < threadNumber; i++)
            {
                Add(new SpinTimerTask(job));
            }
        }

        public async Task Start(MyJobContext context)
        {
            foreach (var item in Tasks)
            {
                await item.Start(context);
            }
        }

        /// <summary>
        /// 是否删除定时器，请根据start时是否重新注册，来决定是否删除
        /// </summary>
        /// <param name="isClear"></param>
        public async Task Stop(bool isClear)
        {
            foreach (var item in Tasks)
            {
                await item.Stop();
            }
            if (isClear)
            {
                Tasks.Clear();
            }
        }

        public event Func<string, Task> OnException;

        private void Add(ITimerTask timerTask)
        {
            Tasks.Add(timerTask);
            timerTask.OnException += TimerTask_OnException1;
        }

        private async Task TimerTask_OnException1(string arg)
        {
            if (OnException != null)
                await OnException(arg);
        }


    }
}
