using Aix.TimerManage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, paramsArray) =>
                 {
                     cts.Cancel();
                     paramsArray.Cancel = true;
                 };
                Run().Wait();
            }

            Console.Read();//控制台程序不能少
            Console.WriteLine("服务已退出");
        }

        private static Task Run(CancellationToken cancellationToken = default(CancellationToken))
        {
            TaskManager taskManager = new TaskManager();
            taskManager.OnException += TaskManager_OnException;
            Console.WriteLine(DateTime.Now);
            //taskManager.RegisterTimerTask(new TimerTaskJob(), 5 * 1000, 5 * 1000);
           //taskManager.RegisterCrontabTask(new CrontabTaskJob(), "*/5 * * * * *");//每5秒
            taskManager.RegisterCrontabTask(new CrontabTaskJob(), "13 * * * * *");
            //taskManager.RegisterSpinTimerTassk(new SpinTimerTaskJob());

            var context = new MyJobContext(cancellationToken);
            return taskManager.Start(context);
        }

        private static Task TaskManager_OnException(string arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
    }
}
