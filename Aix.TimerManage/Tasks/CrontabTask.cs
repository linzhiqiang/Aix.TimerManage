using NCrontab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    /// <summary>
    /// 时间表达式定时job，注意 定时间隔时 要是60能整除的如 1,2,3,5,6,10,60...
    /// </summary>
    public class CrontabTask : ITimerTask
    {
        public event Func<string, Task> OnException;
        private volatile bool IsRunning = true;

        private IMyJob Job = null;
        private CrontabSchedule Schedule = null;

        private DateTime LastExecuteTime = DateTime.MinValue;

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
            TimeSpan nextExecuteTimeSpan = TimeSpan.Zero;
            LastExecuteTime = DateTime.Now;
            while (IsRunning && !context.IsShutdownRequested)
            {
                try
                {
                    nextExecuteTimeSpan = GetNextDueTime(Schedule, LastExecuteTime, DateTime.Now);
                    if (nextExecuteTimeSpan.TotalMilliseconds <= 0)
                    {
                        LastExecuteTime = DateTime.Now;
                        context.Token.ThrowIfCancellationRequested();
                        await ExecuteJob(context);

                        nextExecuteTimeSpan = GetNextDueTime(Schedule, LastExecuteTime, DateTime.Now);
                    }

                    if (nextExecuteTimeSpan > TimeSpan.Zero)
                    {
                        await Task.Delay(nextExecuteTimeSpan);
                    }
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

                }
            }

        }

        private async Task ExecuteJob(MyJobContext context)
        {
            try
            {
                await this.Job.Execute(context);
            }
            catch (Exception ex)
            {
                string message = string.Format("执行定时任务{0}出错：message={1},StackTrace={2}", this.Job.GetType().FullName, ex.Message, ex.StackTrace);
                await this.Exception(message);
            }
        }

        public static TimeSpan GetNextDueTime(CrontabSchedule Schedule, DateTime LastDueTime, DateTime now)
        {
            var nextOccurrence = Schedule.GetNextOccurrence(LastDueTime);
            TimeSpan dueTime = nextOccurrence - now;// DateTime.Now;

            if (dueTime.TotalMilliseconds <= 0)
            {
                dueTime = TimeSpan.Zero;
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
