using AuthentificationWSDL.CMAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthentificationWSDL
{
    public class ConnectionManager
    {
        private const int WAIT_TIME_SERVER_KO = 1;
        private const int WAIT_TIME_AUTH_OK = 10;
        private const int WAIT_TIME_AUTH_KO = 5;
        private const int WAIT_TIME_NOT_RUNNING = 1;


        static private KsmConnection instance = null;

        /// <summary>
        /// Gets the instance of CloudConnectionManager.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        static public KsmConnection Instance
        {
            get
            {
                return ConnectionManager.instance;
            }
        }

        /// <summary>
        /// Starts the manager.
        /// </summary>
        /// <param name="ksm">The KSM.</param>
        /// <param name="stayConnected">if set to <c>true</c> [stay connected].</param>
        static public void StartManager(KeystoneManagerWSDL ksm, bool stayConnected = false)
        {
            ConnectionManager.Dispose();
            ConnectionManager.instance = new ConnectionManager.KsmConnection(ksm, stayConnected);
        }

        /// <summary>
        /// Starts the manager.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="password">The password.</param>
        /// <param name="project">The project.</param>
        /// <param name="stayConnected">if set to <c>true</c> [stay connected].</param>
        static public void StartManager(string login, string password, Projects project, bool stayConnected = false)
        {
            if (ConnectionManager.instance != null &&
                ConnectionManager.instance.ConnectionStatus == Status.Connected &&
                ConnectionManager.instance.Keystone.Username == login && ConnectionManager.instance.Keystone.Project == project)
            {
                return;
            }


            ConnectionManager.Dispose();
            ConnectionManager.instance = new ConnectionManager.KsmConnection(login, password, project, stayConnected);
        }

        /// <summary>
        /// Starts the manager.
        /// </summary>
        /// <param name="login">The login.</param>
        /// <param name="password">The password.</param>
        /// <param name="project">The project.</param>
        /// <param name="stayConnected">if set to <c>true</c> [stay connected].</param>
        static public void ChangeCredentials(string login, string password, Projects project)
        {
            //if (ConnectionManager.instance != null &&
            //    ConnectionManager.instance.Keystone.Username == login && ConnectionManager.instance.Keystone.Project == project)
            //{
            //    return;
            //}

            ConnectionManager.instance.connect(login, password, project);
        }

        /// <summary>
        /// CleansDisconnect the connection.
        /// </summary>
        static public void Disconnect()
        {
            if (ConnectionManager.instance == null) return;

            ConnectionManager.instance.Disconnect();
        }

        /// <summary>
        /// Dispose the connection.
        /// </summary>
        static public void Dispose()
        {
            if (ConnectionManager.instance == null) return;

            ConnectionManager.instance.Disconnect();
            ConnectionManager.instance.Dispose();

            ConnectionManager.instance = null;
        }


        public class KsmConnection
        {
            private static KeystoneManagerWSDL km = null;

            /// <summary>
            /// Gets the keystone.
            /// </summary>
            /// <value>
            /// The keystone.
            /// </value>
            public KeystoneManagerWSDL Keystone
            {
                get
                {
                    return KsmConnection.km;
                }
            }

            private static bool isRunning = false;

            /// <summary>
            /// Gets a value indicating whether this instance is running.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is running; otherwise, <c>false</c>.
            /// </value>
            public bool IsRunning
            {
                get
                {
                    return KsmConnection.isRunning;
                }
            }

            /// <summary>
            /// Gets the attempt counter.
            /// </summary>
            /// <value>
            /// The attempt counter.
            /// </value>
            public int AttemptCounter { get; private set; }

            /// <summary>
            /// Gets the connection status.
            /// </summary>
            /// <value>
            /// The connection status.
            /// </value>
            public Status ConnectionStatus { get; private set; }

            /// <summary>
            /// The thread for the processing loop
            /// </summary>
            private Thread thLoop = null;

            /// <summary>
            /// The kill thread (true = thread to be killed)
            /// </summary>
            private static bool killThread = false;

            /// <summary>
            /// Prevents a default instance of the <see cref="KsmConnection"/> class from being created.
            /// </summary>
            KsmConnection()
            {
                this.AttemptCounter = 0;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="KsmConnection"/> class.
            /// </summary>
            /// <param name="project">The project.</param>
            KsmConnection(Projects project) : this()
            {
                this.connect("", "", project);
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="KsmConnection"/> class.
            /// </summary>
            ~KsmConnection()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="KsmConnection"/> class.
            /// </summary>
            /// <param name="login">The login.</param>
            /// <param name="password">The password.</param>
            /// <param name="project">The project.</param>
            /// <param name="stayConnected">if set to <c>true</c> [stay connected].</param>
            public KsmConnection(string login, string password, Projects project, bool stayConnected = false) : this(project)
            {
                Thread.Sleep(1000);

                if (!this.Keystone.IsServerAvailable)
                {
                    this.ConnectionStatus = Status.ServerUnavailable;
                }
                else if (string.IsNullOrEmpty(login))
                {
                    this.ConnectionStatus = Status.Disconnected;
                }
                else
                {
                    this.ConnectionStatus = Status.ConnectionPending;
                    this.connect(login, password, project);

                    if (stayConnected) this.startMonitoring();
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="KsmConnection"/> class.
            /// </summary>
            /// <param name="ksm">The KSM.</param>
            /// <param name="stayConnected">if set to <c>true</c> [stay connected].</param>
            public KsmConnection(KeystoneManagerWSDL ksm, bool stayConnected = false) : this(ksm.Project)
            {
                if (!ksm.IsServerAvailable)
                {
                    this.ConnectionStatus = Status.ServerUnavailable;
                }
                else
                {
                    this.ConnectionStatus = Status.ConnectionPending;
                    this.connect(ksm);

                    if (stayConnected) this.startMonitoring();
                }
            }

            private void startMonitoring()
            {
                KsmConnection.killThread = false;

                this.thLoop = new Thread(monitorConnection);
                this.thLoop.Start();

                KsmConnection.isRunning = true;
            }

            /// <summary>
            /// Monitors the connection.
            /// </summary>
            private void monitorConnection()
            {
                while (!killThread) // runs while the Thread is not killed
                {
                    try
                    {
                        if (!KsmConnection.isRunning)
                        {
                            Thread.Sleep(ConnectionManager.WAIT_TIME_NOT_RUNNING * 1000);
                            continue;
                        }

                        // stop if there are too many connection attempts
                        if (this.AttemptCounter > 5)
                        {
                            this.ConnectionStatus = Status.TooManyAttempt;
                            KsmConnection.isRunning = false;
                        }

                        // continue to try the connection while the server is not available
                        if (!this.Keystone.IsServerAvailable)
                        {
                            this.ConnectionStatus = Status.ServerUnavailable;

                            Thread.Sleep(ConnectionManager.WAIT_TIME_SERVER_KO * 1000);
                            continue;
                        }


                        // if the credentials are good but the Token session expired
                        if (this.Keystone.HasToken && !this.Keystone.IsValid)
                        {
                            this.ConnectionStatus = Status.KeystoneNotAlive;
                            this.connect(Keystone.Username, Keystone.Password, Keystone.Project);

                            Thread.Sleep(ConnectionManager.WAIT_TIME_AUTH_KO * 1000);
                            continue;
                        }

                        // if the credentials are not good (bad login or password or project)
                        if (!this.Keystone.HasToken && !this.Keystone.IsValid)
                        {
                            this.ConnectionStatus = Status.BadCredentials;
                            KsmConnection.isRunning = false;
                            continue;
                        }

                        // if all is good :)
                        this.ConnectionStatus = Status.Connected;
                        Thread.Sleep(ConnectionManager.WAIT_TIME_AUTH_OK * 1000);
                    }

                    catch
                    {
                        // ERROR
                        this.ConnectionStatus = Status.KsmError;
                        Thread.Sleep(ConnectionManager.WAIT_TIME_SERVER_KO * 1000);
                    }
                }

            }


            /// <summary>
            /// Connects the specified keystone.
            /// </summary>
            /// <param name="keystone">The keystone.</param>
            internal void connect(KeystoneManagerWSDL keystone)
            {
                this.AttemptCounter = 0;
                KsmConnection.km = keystone;

                if (this.thLoop == null || !this.thLoop.IsAlive) startMonitoring();
                else KsmConnection.isRunning = true;
            }

            /// <summary>
            /// Connects the specified login.
            /// </summary>
            /// <param name="login">The login.</param>
            /// <param name="password">The password.</param>
            /// <param name="project">The project.</param>
            internal void connect(string login, string password, Projects project)
            {
                this.AttemptCounter = 0;
                KsmConnection.km = this.getKsm(login, password, project);

                if (this.thLoop == null || !this.thLoop.IsAlive) startMonitoring();
                else KsmConnection.isRunning = true;
            }

            /// <summary>
            /// Gets the KSM from credentials.
            /// </summary>
            /// <param name="login">The login.</param>
            /// <param name="password">The password.</param>
            /// <param name="project">The project.</param>
            /// <returns></returns>
            private KeystoneManagerWSDL getKsm(string login, string password, Projects project)
            {
                this.AttemptCounter++;
                switch (project)
                {
                    case Projects.CAxMan:
                        return new KeystoneManagerWSDL_CM(login, password, Enum.GetName(typeof(Projects), project).ToLower());
                    default:
                        return new KeystoneManagerWSDL_CM("", "", null);
                }
            }

            /// <summary>
            /// Disconnects this instance.
            /// </summary>
            internal void Disconnect()
            {
                if (this.Keystone != null)
                {
                    this.Keystone.Disconnect();
                }
                this.AttemptCounter = 0;
                this.ConnectionStatus = Status.Disconnected;


                KsmConnection.isRunning = false;
            }

            internal void Dispose()
            {
                this.Keystone.Dispose();

                if (this.thLoop == null) return;

                this.thLoop.Abort();
                KsmConnection.killThread = true;
                this.thLoop = null;
            }
        }

        public enum Status
        {
            Disconnected = 0,
            ConnectionPending = 1,
            Connected = 2,
            TooManyAttempt = 4,
            KeystoneNotAlive = 8,
            KsmError = 16,
            ServerUnavailable = 32,
            BadCredentials = 64
        }
    }
}
