using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Aix.TimerManage
{

    /// <summary>
    /// 定时任务
    /// </summary>
    public class TimerTask : ITimerTask
    {
        public event Func<string, Task> OnException;
        private volatile bool IsRunning = true;

        private IMyJob Job = null;
        private int DueTime;
        private int Period;
        public TimerTask(IMyJob job, int dueTime, int period)
        {
            this.Job = job;
            this.DueTime = dueTime;
            this.Period = period;

        }

        public Task Start(MyJobContext context)
        {
            if (this.Job == null) return Task.CompletedTask;
            this.IsRunning = true;
            return Task.Factory.StartNew(async () =>
            {
                if (DueTime > 0) await Task.Delay(DueTime);
                await Execute(context);
            }, TaskCreationOptions.LongRunning);
        }

        private async Task Execute(MyJobContext context)
        {
            while (IsRunning && !context.IsShutdownRequested)
            {
                try
                {
                    var duration = Stopwatch.StartNew();
                    context.Token.ThrowIfCancellationRequested();
                    await DoWork(context);
                    duration.Stop();
                    var performanceDuration = duration.ElapsedMilliseconds;

                    var dealy = Period - performanceDuration;
                    if (dealy > 0)
                        await Task.Delay(TimeSpan.FromMilliseconds(dealy));
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
            }
        }

        private async Task DoWork(MyJobContext context)
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

        public Task Stop()
        {
            this.IsRunning = false;
            return Task.CompletedTask;
        }

        private async Task Exception(string message)
        {
            if (this.OnException != null)
            {
                await this.OnException(message);
            }
        }
    }
}
