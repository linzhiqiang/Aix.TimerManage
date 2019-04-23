using NCrontab;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    /// <summary>
    /// 废弃了，请使用CrontabTask
    /// </summary>
    public class CrontabByTimerTask : ITimerTask
    {
        public event Func<string, Task> OnException;
        private System.Threading.Timer Timer = null;
        private IMyJob Job = null;
        private CrontabSchedule Schedule = null;

        private DateTime LastDueTime = DateTime.MinValue;

        public CrontabByTimerTask(string expression, IMyJob job)
        {
            this.Job = job;
            var options = new CrontabSchedule.ParseOptions
            {
                IncludingSeconds = expression.Split(' ').Length > 5,
            };
            Schedule = CrontabSchedule.Parse(expression, options);
        }

        public Task Start(MyJobContext context)
        {
            if (this.Job == null)
                return Task.CompletedTask;

            LastDueTime = DateTime.Now;
            var dueTime = GetNextDueTime();
            this.Timer = new System.Threading.Timer(CallBack, context, dueTime, TimeSpan.Zero);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            if (this.Timer != null)
            {
                try
                {
                    this.Timer.Dispose();
                }
                catch
                {
                }
            }
            return Task.CompletedTask;
        }

        #region private

        private TimeSpan GetNextDueTime()
        {
            var nextOccurrence = Schedule.GetNextOccurrence(LastDueTime);
            TimeSpan dueTime = nextOccurrence - DateTime.Now;
            LastDueTime = nextOccurrence;

            if (dueTime.TotalMilliseconds < 0)
            {
                dueTime = TimeSpan.Zero;
                LastDueTime = DateTime.Now;
            }

            return dueTime;
        }

        private void CallBack(object state)
        {
            Execute(state).Wait();
        }
        private async Task Execute(object state)
        {
            MyJobContext context = state as MyJobContext;
            try
            {
                await this.Job.Execute(context);
            }
            catch (Exception ex)
            {
                string message = string.Format("执行定时任务{0}出错：message={1},StackTrace={2}", this.Job.GetType().FullName, ex.Message, ex.StackTrace);
                await this.Exception(message);
            }
            finally
            {
                var dueTime = GetNextDueTime();
                this.Timer.Change(dueTime, TimeSpan.Zero);
            }
        }

        private async Task Exception(string message)
        {
            if (this.OnException != null)
            {
                await this.OnException(message);
            }
        }

        #endregion
    }
}
