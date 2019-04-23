using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.TimerManage
{
    /// <summary>
    /// 自旋任务 job中自己控制delay
    /// </summary>
    public class SpinTimerTask : ITimerTask
    {
        private IMyJob Job = null;
        private volatile bool IsRunning = true;
        public SpinTimerTask(IMyJob job)
        {
            this.Job = job;
        }

        public event Func<string, Task> OnException;

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
            }

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
