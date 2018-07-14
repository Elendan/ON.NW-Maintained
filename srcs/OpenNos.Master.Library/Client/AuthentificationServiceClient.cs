using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Core.Networking.Communication.Scs.Communication;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.ScsServices.Client;
using OpenNos.Data;
using OpenNos.Master.Library.Interface;

namespace OpenNos.Master.Library.Client
{
    public class AuthentificationServiceClient : IAuthentificationService
    {
        #region Members

        private static AuthentificationServiceClient _instance;

        private readonly IScsServiceClient<IAuthentificationService> _client;

        #endregion

        #region Instantiation

        public AuthentificationServiceClient()
        {
            string ip = ConfigurationManager.AppSettings["MasterIP"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["MasterPort"]);
            _client = ScsServiceClientBuilder.CreateClient<IAuthentificationService>(new ScsTcpEndPoint(ip, port));
            System.Threading.Thread.Sleep(1000);
            while (_client.CommunicationState != CommunicationStates.Connected)
            {
                try
                {
                    _client.Connect();
                }
                catch (Exception)
                {
                    Logger.Log.Error(Language.Instance.GetMessageFromKey("RETRY_CONNECTION"));
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        #endregion

        #region Properties

        public static AuthentificationServiceClient Instance => _instance ?? (_instance = new AuthentificationServiceClient());

        public CommunicationStates CommunicationState => _client.CommunicationState;

        #endregion

        #region Methods

        public bool Authenticate(string authKey) => _client.ServiceProxy.Authenticate(authKey);

        public AccountDTO ValidateAccount(string userName, string passHash) => _client.ServiceProxy.ValidateAccount(userName, passHash);

        public CharacterDTO ValidateAccountAndCharacter(string userName, string characterName, string passHash) => _client.ServiceProxy.ValidateAccountAndCharacter(userName, characterName, passHash);

        #endregion
    }
}
