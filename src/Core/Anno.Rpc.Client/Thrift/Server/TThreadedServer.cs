using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Thrift.Collections;
using Thrift.Protocol;
using Thrift.Transport;

namespace Thrift.Server
{
    /// <summary>
    /// Server that uses C# threads (as opposed to the ThreadPool) when handling requests.
    /// </summary>
    public class TThreadedServer : TServer
    {
        private const Int32 DEFAULT_MAX_THREADS = 100;
        private volatile Boolean stop = false;
        private readonly Int32 maxThreads;

        private Queue<TTransport> clientQueue;
        private THashSet<Thread> clientThreads;
        private readonly Object clientLock;
        private Thread workerThread;

        public Int32 ClientThreadsCount => clientThreads.Count;

        public TThreadedServer(TProcessor processor, TServerTransport serverTransport)
            : this(new TSingletonProcessorFactory(processor), serverTransport,
             new TTransportFactory(), new TTransportFactory(),
             new TBinaryProtocol.Factory(), new TBinaryProtocol.Factory(),
             DEFAULT_MAX_THREADS, DefaultLogDelegate)
        {
        }

        public TThreadedServer(TProcessor processor, TServerTransport serverTransport, LogDelegate logDelegate)
            : this(new TSingletonProcessorFactory(processor), serverTransport,
             new TTransportFactory(), new TTransportFactory(),
             new TBinaryProtocol.Factory(), new TBinaryProtocol.Factory(),
             DEFAULT_MAX_THREADS, logDelegate)
        {
        }

        public TThreadedServer(TProcessor processor, TServerTransport serverTransport, LogDelegate logDelegate, Int32 maxThreads=DEFAULT_MAX_THREADS)
           : this(new TSingletonProcessorFactory(processor), serverTransport,
            new TTransportFactory(), new TTransportFactory(),
            new TBinaryProtocol.Factory(), new TBinaryProtocol.Factory(),
            maxThreads, logDelegate)
        {
        }


        public TThreadedServer(TProcessor processor,
                     TServerTransport serverTransport,
                     TTransportFactory transportFactory,
                     TProtocolFactory protocolFactory)
            : this(new TSingletonProcessorFactory(processor), serverTransport,
             transportFactory, transportFactory,
             protocolFactory, protocolFactory,
             DEFAULT_MAX_THREADS, DefaultLogDelegate)
        {
        }

        public TThreadedServer(TProcessorFactory processorFactory,
               TServerTransport serverTransport,
               TTransportFactory transportFactory,
               TProtocolFactory protocolFactory)
            : this(processorFactory, serverTransport,
             transportFactory, transportFactory,
             protocolFactory, protocolFactory,
             DEFAULT_MAX_THREADS, DefaultLogDelegate)
        {
        }
        public TThreadedServer(TProcessorFactory processorFactory,
                     TServerTransport serverTransport,
                     TTransportFactory inputTransportFactory,
                     TTransportFactory outputTransportFactory,
                     TProtocolFactory inputProtocolFactory,
                     TProtocolFactory outputProtocolFactory,
                     Int32 maxThreads, LogDelegate logDel)
          : base(processorFactory, serverTransport, inputTransportFactory, outputTransportFactory,
              inputProtocolFactory, outputProtocolFactory, logDel)
        {
            this.maxThreads = maxThreads;
            clientQueue = new Queue<TTransport>();
            clientLock = new Object();
            clientThreads = new THashSet<Thread>();
        }

        /// <summary>
        /// Use new Thread for each new client connection. block until numConnections &lt; maxThreads.
        /// </summary>
        public override void Serve()
        {
            try
            {
                //start worker thread
                workerThread = new Thread(new ThreadStart(Execute));
                workerThread.Start();
                serverTransport.Listen();
            }
            catch (TTransportException ttx)
            {
                //logDelegate("Error, could not listen on ServerTransport: " + ttx);
                Anno.Log.Log.WriteLine("Error, could not listen on ServerTransport: " + ttx);
                return;
            }

            //Fire the preServe server event when server is up but before any client connections
            if (serverEventHandler != null)
                serverEventHandler.preServe();

            while (!stop)
            {
                var failureCount = 0;
                try
                {
                    var client = serverTransport.Accept();
                    lock (clientLock)
                    {
                        clientQueue.Enqueue(client);
                        Monitor.Pulse(clientLock);
                    }
                }
                catch (TTransportException ttx)
                {
                    if (!stop || ttx.Type != TTransportException.ExceptionType.Interrupted)
                    {
                        ++failureCount;
                        logDelegate(ttx.ToString());
                    }

                }
            }

            if (stop)
            {
                try
                {
                    serverTransport.Close();
                }
                catch (TTransportException ttx)
                {
                    logDelegate("TServeTransport failed on close: " + ttx.Message);
                }
                stop = false;
            }
        }

        /// <summary>
        /// Loops on processing a client forever
        /// </summary>
        private void Execute()
        {
            while (!stop)
            {
                TTransport client;
                Thread t;
                lock (clientLock)
                {
                    //don't dequeue if too many connections
                    while (clientThreads.Count >= maxThreads)
                    {
                        Monitor.Wait(clientLock);
                    }

                    while (clientQueue.Count == 0)
                    {
                        Monitor.Wait(clientLock);
                    }

                    client = clientQueue.Dequeue();
                    t = new Thread(new ParameterizedThreadStart(ClientWorker));
                    clientThreads.Add(t);
                }
                //start processing requests from client on new thread
                t.Start(client);
            }
        }

        private void ClientWorker(Object context)
        {
            using (var client = (TTransport)context)
            {
                var processor = processorFactory.GetProcessor(client);
                TTransport inputTransport = null;
                TTransport outputTransport = null;
                TProtocol inputProtocol = null;
                TProtocol outputProtocol = null;
                Object connectionContext = null;
                try
                {
                    try
                    {
                        inputTransport = inputTransportFactory.GetTransport(client);
                        outputTransport = outputTransportFactory.GetTransport(client);
                        inputProtocol = inputProtocolFactory.GetProtocol(inputTransport);
                        outputProtocol = outputProtocolFactory.GetProtocol(outputTransport);

                        //Recover event handler (if any) and fire createContext server event when a client connects
                        if (serverEventHandler != null)
                            connectionContext = serverEventHandler.createContext(inputProtocol, outputProtocol);

                        //Process client requests until client disconnects
                        while (!stop)
                        {
                            if (!inputTransport.Peek())
                                break;

                            //Fire processContext server event
                            //N.B. This is the pattern implemented in C++ and the event fires provisionally.
                            //That is to say it may be many minutes between the event firing and the client request
                            //actually arriving or the client may hang up without ever makeing a request.
                            if (serverEventHandler != null)
                                serverEventHandler.processContext(connectionContext, inputTransport);
                            //Process client request (blocks until transport is readable)
                            if (!processor.Process(inputProtocol, outputProtocol))
                                break;
                        }
                    }
                    catch (TTransportException)
                    {
                        //Usually a client disconnect, expected
                    }
                    catch (Exception x)
                    {
                        //Unexpected
                        logDelegate("Error: " + x);
                    }

                    //Fire deleteContext server event after client disconnects
                    if (serverEventHandler != null)
                        serverEventHandler.deleteContext(connectionContext, inputProtocol, outputProtocol);

                    lock (clientLock)
                    {
                        clientThreads.Remove(Thread.CurrentThread);
                        Monitor.Pulse(clientLock);
                    }

                }
                finally
                {
                    //Close transports
                    if (inputTransport != null)
                        inputTransport.Close();
                    if (outputTransport != null)
                        outputTransport.Close();

                    // disposable stuff should be disposed
                    if (inputProtocol != null)
                        inputProtocol.Dispose();
                    if (outputProtocol != null)
                        outputProtocol.Dispose();
                }
            }
        }

        public override void Stop()
        {
            stop = true;
            serverTransport.Close();
            //clean up all the threads myself
#if !NETSTANDARD
            workerThread.Abort();
            foreach (Thread t in clientThreads)
            {
                t.Abort();
            }
#endif
        }
    }
}