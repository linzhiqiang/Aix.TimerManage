using NCrontab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    public class CrontabTask : ITimerTask
    {
        public event Func<string, Task> OnException;
        private volatile bool IsRunning = true;

        private IMyJob Job = null;
        private CrontabSchedule Schedule = null;

        private DateTime LastDueTime = DateTime.MinValue;

        public CrontabTask(string expression, IMyJob job)
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
            if (this.Job == null) return Task.CompletedTask;
            this.IsRunning = true;
            return Task.Factory.StartNew(async () =>
            {
                LastDueTime = DateTime.Now;
                var dueTime = GetNextDueTime();
                if (dueTime.TotalMilliseconds > 0)
                {
                    await Task.Delay((int)dueTime.TotalMilliseconds);
                }

                await Execute(context);
            }, TaskCreationOptions.LongRunning);
        }

        public Task Stop()
        {
            this.IsRunning = false;
            return Task.CompletedTask;
        }


        #region  private

        private async Task Execute(MyJobContext context)
        {
            while (IsRunning && !context.IsShutdownRequested)
            {
                try
                {
                    context.Token.ThrowIfCancellationRequested();
                    await this.Job.Execute(context);
                }
                catch (OperationCanceledException)
                {
                    await this.Stop();
                    break;
                }
                catch (Exception ex)
                {
                    string message = string.Format("执行定时任务{0}出错：message={1},StackTrace={2}", this.Job.GetType().FullName, ex.Message, ex.StackTrace);
                    await this.Exception(message);
                }
                finally
                {
                    var dueTime = GetNextDueTime();
                    if (dueTime.TotalMilliseconds > 0)
                    {
                        await Task.Delay((int)dueTime.TotalMilliseconds);
                    }
                }
            }

        }

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
