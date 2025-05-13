using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Opc.UaFx;
using Opc.UaFx.Client;

namespace POC.Model
{
    public class OPCUA
    {
        /// <summary>
        /// Refresh the GUI when invoke.
        /// </summary>
        public event EventHandler Refresh;

        /// <summary>
        /// Start to poll on the OPC-UA server.
        /// </summary>
        protected Thread PollingThread = null;

        protected OpcClient OpcClient = new OpcClient();
        public OPCUA() 
        {
            string user = "admin";
            string pw = "wago";

            OpcSecurityPolicy opcSecurityPolicy = new OpcSecurityPolicy(OpcSecurityMode.SignAndEncrypt, OpcSecurityAlgorithm.Basic256Sha256);
            OpcClient = new OpcClient("opc.tcp://157.26.99.78:4840", opcSecurityPolicy);
            OpcClient.Security.UserIdentity = new OpcClientIdentity(user, pw);
            OpcClient.Connect();
        }
        
        /// <summary>
        /// Polling actions
        /// </summary>
        /// <returns></returns>
        protected async Task PollingThreadWork() 
        {
            while (true)
            {
                //If the object move
                if(ReadObject<bool>("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.xTriggerStart", 4))
                { 
                    App.HasMoveFinished = false;
                }
                else 
                {
                    App.HasMoveFinished = true;
                }

                App.DesiredPos = ReadObject<int>("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.iTargetPosition", 4);
                App.ActualPos = ReadObject<int>("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.iActualPosition", 4);

                //Force GUI to refresh
                this.Refresh.Invoke(this, EventArgs.Empty);

                Thread.Sleep(100);
            }
        }
        
        /// <summary>
        /// Start the polling thread
        /// </summary>
        public void PollingThreadStart() 
        {
            PollingThread = new Thread(new ThreadStart(async () => await PollingThreadWork()));
            PollingThread.Start();
        }

        /// <summary>
        /// Read an OPC-UA var
        /// </summary>
        /// <typeparam name="T">Value of the var on serv</typeparam>
        /// <param name="objectId">Obj ID</param>
        /// <param name="iNamespace"></param>
        /// <returns></returns>
        public T ReadObject<T>(string objectId, int iNamespace)
        {
            OpcNodeId obj = new OpcNodeId(objectId, iNamespace);
            var val = OpcClient.ReadNode(obj);
            return val.As<T>();
        }

        /// <summary>
        /// Set the desired target on the OPC-UA serv
        /// </summary>
        /// <param name="i"></param>
        /// <param name="val"></param>
        public void OpcRequestSetTarget(int i, int val)
        {
            if(OpcClient == null) 
            {
                Debug.WriteLine("Client NULL");
                return;
            }
            if (OpcClient.State != OpcClientState.Connected)
            {
                Debug.WriteLine("Client NOT CONNECTED");
                return;
            }
            else 
            {
                Debug.WriteLine(OpcClient.State);
            }

            //Change the value of the var on the serv
            WriteObject("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.iTargetPosition", i, (short)val);
        }

        public void UpdateBoolToTrue(int i) 
        {
            WriteObject("|var|750-8000 Basic Controller 100 2ETH ECO.Application.Var_OPC_UA.xTriggerStart", i, true);
        }

        public OpcStatus WriteObject(string objId, int nameSpace, short newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            return OpcClient.WriteNode(obj, newValue);
        }

        public OpcStatus WriteObject(string objId, int nameSpace, bool newValue)
        {
            OpcNodeId obj = new OpcNodeId(objId, nameSpace);
            return OpcClient.WriteNode(obj, newValue);
        }
    }
}
