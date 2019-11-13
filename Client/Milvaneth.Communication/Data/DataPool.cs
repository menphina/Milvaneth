using System;
using Milvaneth.Common;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Communication.Data
{
    public class DataPool
    {
        private readonly ConcurrentDictionary<int, DataContainer> _memTable;
        private readonly ConcurrentQueue<MilvanethProtocol> _dataQueue;
        private readonly ExchangeProcedure _exchange;

        public delegate void DataUploadedDelegate(string info);

        private readonly DataUploadedDelegate _onDataUploaded;

        public DataPool(DataUploadedDelegate onDataUploaded)
        {
            _memTable = new ConcurrentDictionary<int, DataContainer>();
            _dataQueue = new ConcurrentQueue<MilvanethProtocol>();
            _exchange = new ExchangeProcedure();

            _onDataUploaded = onDataUploaded;

            Task.Run(UploadSupervisor);
        }

        public void SinkData(int identifier, PackedResult pr)
        {
            var container = _memTable.GetOrAdd(identifier, new DataContainer(_dataQueue));
            container.TryAdd(pr);
        }

        public MilvanethContext GetContext(int identifier)
        {
            return _memTable.TryGetValue(identifier, out var container) ? container?.Context?.Copy() : null;
        }

        private void UploadSupervisor()
        {
            var taskList = new Task[8];
            var cancelList = new CancellationTokenSource[8];

            for (var i = 0; i < taskList.Length; i++)
            {
                cancelList[i] = new CancellationTokenSource(60000);
                taskList[i] = Task.Run(UploadTask, cancelList[i].Token);
            }

            for (;;)
            {
                for (var i = 0; i < taskList.Length; i++)
                {
                    if (taskList[i].IsCompleted || taskList[i].IsCanceled || taskList[i].IsFaulted || cancelList[i].IsCancellationRequested)
                    {
                        try { taskList[i].Dispose(); } catch { /* ignored */ }
                        try { cancelList[i].Dispose(); } catch { /* ignored */ }

                        try
                        {
                            cancelList[i] = new CancellationTokenSource(60000);
                            taskList[i] = Task.Run(UploadTask, cancelList[i].Token);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void UploadTask()
        {
            if (_dataQueue.TryDequeue(out var data))
            {
                try
                {
                    var ret = _exchange.Step1(data);

                    if (ret == 04_0511 && ApiVendor.HasToken())
                    {
                        ApiVendor.RenewToken(DataHolder.Username, ref DataHolder.RenewToken);
                        _exchange.Step1(data);
                    }

                    if (!CheckVendor.NotValidResponseCode(ret))
                    {
                        _onDataUploaded?.Invoke($"Data Uploaded: {((PackedResult)data.Data).Type.ToString()}");
                    }
                }
                catch (HttpRequestException e)
                {
                    try
                    {
                        if ((int)e.Data["StatusCode"] == 511 && ApiVendor.HasToken())
                        {
                            ApiVendor.RenewToken(DataHolder.Username, ref DataHolder.RenewToken);
                            _exchange.Step1(data);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
                catch
                {
                    // ignored
                }
            }

            Thread.Sleep(250);
        }
    }
}
