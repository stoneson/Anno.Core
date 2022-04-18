using System;
using System.Collections.Generic;

namespace Anno.Plugs.DTransactionService
{
    using Anno.Const.Enum;
    using Anno.CronNET;
    using EngineData;
    using System.Linq;

    public class DTransactionBootstrap : IPlugsConfigurationBootstrap
    {
        private static readonly CronDaemon CronDaemon = new CronDaemon();
        public void ConfigurationBootstrap()
        {
            /*
             * 每个一段时间检测是否有超时的 Sagas
             */
            CronDaemon.AddJob("* * * * * ? *", () =>
            {
                if (DTransactionManager.Sagas.Keys.Count > 0)
                {
                    var sagas = DTransactionManager.Sagas.Values.Where(it => it.Deadline < DateTime.Now).ToList();
                    foreach (var saga in sagas)
                    {
                        Recovery(saga);
                        if (saga.Sagas.Count > 0)
                        {
                            DTransactionManager.Sagas.TryRemove(saga.Sagas.First().sagaGlobalId, out SagaTxs sagaTxs);
                        }
                    }
                }
            });
            if (CronDaemon.Status == DaemonStatus.Stop)
            {
                CronDaemon.Start();
            }
        }

        public void PreConfigurationBootstrap()
        {

        }
        private void Recovery(SagaTxs sagaTxs)
        {
            var rpc = Loader.IocLoader.Resolve<IRpcConnector>();
            if (rpc != null)
            {
                while (sagaTxs.Sagas.TryPop(out SagaTx tx))
                {
                    try
                    {
                        var response = tx.sagaInput;
                        string channel, router, method;
                        channel = tx.sagaInput[Eng.NAMESPACE];
                        router = tx.sagaInput[Eng.CLASS];
                        method = tx.recover;
                        response.Add("sagaRlt", tx.sagaRlt);

                        if (response.ContainsKey("TraceId"))
                        {
                            response.Remove("TraceId");
                        }

                        if (response.ContainsKey("PreTraceId"))
                        {
                            response.Remove("PreTraceId");
                        }
                        #region 路由信息

                        if (response.ContainsKey(Eng.NAMESPACE))
                        {
                            response[Eng.NAMESPACE] = channel;
                        }
                        else
                        {
                            response.Add(Eng.NAMESPACE, channel);
                        }

                        if (response.ContainsKey(Eng.CLASS))
                        {
                            response[Eng.CLASS] = router;
                        }
                        else
                        {
                            response.Add(Eng.CLASS, router);
                        }

                        if (response.ContainsKey(Eng.METHOD))
                        {
                            response[Eng.METHOD] = method;
                        }
                        else
                        {
                            response.Add(Eng.METHOD, method);
                        }
                        #endregion
                        rpc.BrokerDns(response);
#if DEBUG
                        Log.Log.WriteLine($"channel:{channel},router:{router},method:{method}       SagaRecovery!");
#endif
                    }
                    catch (Exception ex)
                    {
                        Log.Log.Error(ex, typeof(DTransactionManager));
                    }
                }
            }
        }

    }
}
