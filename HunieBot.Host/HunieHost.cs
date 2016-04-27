using Discord;
using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Injection;
using HunieBot.Host.Interfaces;
using HunieBot.Host.Internal;
using HunieBot.Host.Permissions;
using Ninject;
using Ninject.Injection;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HunieBot.Host
{

    /// <summary>
    ///     The <see cref="HunieHost"/> is responsible for creating all instances of command handling and bots.
    /// </summary>
    public sealed class HunieHost : IDisposable
    {
        private readonly static string HunieBotDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HunieBot");
        private readonly static string ConfigurationFile = Path.Combine(HunieBotDataFolder, "configuration.json");
        private readonly static string UserPermissionsFile = Path.Combine(HunieBotDataFolder, "userpermissions.json");


        private readonly IHunieUserPermissions _userPermissions;
        private readonly INinjectModule _loadedModule;
        private readonly IKernel _ninject;
        private readonly ILogging _logger;
        private readonly IList<HunieWrapper> _wrappers;
        private readonly DiscordClient _discordClientConnection;



        /// <summary>
        ///     Gets or sets the <see cref="HunieConfiguration"/> for this <see cref="HunieHost"/>.
        /// </summary>
        public HunieConfiguration Configuration { get; set; } = new HunieConfiguration();



        /// <summary>
        ///     Creates a new instance of the <see cref="HunieHost"/>.
        /// </summary>
        /// <param name="token">The Discord Bot Token that <see cref="HunieHost"/> is to assume control over</param>
        public HunieHost()
        {
            var dc = new DiscordConfigBuilder
            {
                AppName = "HunieBot",
                MessageCacheSize = 10,
                ReconnectDelay = (int)TimeSpan.FromSeconds(10).TotalMilliseconds
            };
#if DEBUG
            _loadedModule = new Debug();
#else
            _loadedModule = new Release();
#endif
            _discordClientConnection = new DiscordClient(dc);
            _ninject = new StandardKernel(_loadedModule);
            _logger = _ninject.Get<ILogging>();
            _userPermissions = _ninject.Get<HunieUserPermissions>();
            _wrappers = new List<HunieWrapper>();
            _userPermissions.Load(UserPermissionsFile);
            Configuration.Load(ConfigurationFile);
            LoadInternalBotInstances();
            SetAdditionalBindings();
            FindHunieBotInstances();
        }



        /// <summary>
        ///     Asynchronously starts <see cref="HunieHost"/>
        /// </summary>
        /// <returns>A promise to start <see cref="HunieHost"/></returns>
        public async Task Start()
        {
            await _discordClientConnection.Connect(Configuration.DiscordToken);
            _discordClientConnection.SetGame("I'm HunieBot! PM me \"!help\" for more details!");
            _discordClientConnection.MessageReceived += _incomingMessage;
            _discordClientConnection.UserJoined += _userJoined;
            _discordClientConnection.UserLeft += _userLeft;
            _discordClientConnection.JoinedServer += _joinedServer;
            _discordClientConnection.LeftServer += _departedServer;
            _discordClientConnection.UserBanned += _userBanned;
            _discordClientConnection.UserUnbanned += _userUnbanned;
            _discordClientConnection.ChannelCreated += _channelCreated;
            _discordClientConnection.ChannelUpdated += _channelUpdated;
            _discordClientConnection.ChannelDestroyed += _channelDestroyed;
        }

        /// <summary>
        ///     Asynchronously stops <see cref="HunieHost"/>
        /// </summary>
        /// <returns>A promise to stop <see cref="HunieHost"/></returns>
        public async Task Stop()
        {
            _userPermissions.Save(UserPermissionsFile);
            Configuration.Save(ConfigurationFile);
            await _discordClientConnection.Disconnect();
            _discordClientConnection.MessageReceived -= _incomingMessage;
            _discordClientConnection.UserJoined -= _userJoined;
            _discordClientConnection.UserLeft -= _userLeft;
            _discordClientConnection.JoinedServer -= _joinedServer;
            _discordClientConnection.LeftServer -= _departedServer;
            _discordClientConnection.UserBanned -= _userBanned;
            _discordClientConnection.UserUnbanned -= _userUnbanned;
            _discordClientConnection.ChannelCreated -= _channelCreated;
            _discordClientConnection.ChannelUpdated -= _channelUpdated;
            _discordClientConnection.ChannelDestroyed -= _channelDestroyed;
        }

        /// <summary>
        ///     Registers a type with <see cref="HunieHost"/> that initializes all bindings.
        /// </summary>
        /// <param name="t">The <see cref="Type"/> to register</param>
        public void RegisterHunieBotType(Type t)
        {
            if (_wrappers.Any(c => c.Type == t)) return;
            var hea = t.GetCustomAttribute<HunieBotAttribute>();
            if (hea == null) return;
            var instance = _ninject.Get(t);
            var wrapper = new HunieWrapper(hea, instance, _ninject.Get<ILogging>(), _ninject.Get<IKernel>(), _ninject.Get<IHunieUserPermissions>());
            _wrappers.Add(wrapper);
        }

        /// <summary>
        ///     Disposes of <see cref="HunieHost"/>
        /// </summary>
        public void Dispose()
        {
            _discordClientConnection.Dispose();
        }



        /// <summary>
        ///     Enumerates a folder and returns all possible DLL files as <see cref="Assembly"/>
        /// </summary>
        /// <param name="folder">The folder to iterate</param>
        /// <returns>Collection of <see cref="Assembly"/></returns>
        private IEnumerable<Assembly> GetAssembliesInFolder(string folder)
        {
            foreach(var assembly in Directory.EnumerateFiles(folder, "*.dll"))
            {
                Assembly a;
                try
                {
                    a = Assembly.LoadFile(assembly);
                }
                catch
                {
                    continue;
                }
                yield return a;
            }
        }
        
        /// <summary>
        ///     Finds and loads any and all types that are decorated with <see cref="HunieBotAttribute"/>.
        /// </summary>
        private void FindHunieBotInstances()
        {
            var typeList = new List<Type>();
            foreach (var candidateTypes in GetAssembliesInFolder(Environment.CurrentDirectory).Select(a => a.GetTypes()))
                typeList.AddRange(candidateTypes.Where(t => t.GetCustomAttributes(typeof(HunieBotAttribute), false).Any()));

            // We've got our candidates! Let's do this folks.
            foreach (var type in typeList) RegisterHunieBotType(type);
        }

        private void LoadInternalBotInstances()
        {

            // These are bindings for internal modules only.
            _ninject.Bind<IHunieUserPermissions>().ToConstant(_userPermissions);

            // Let's create our internal instances.
            RegisterHunieBotType(typeof(Internal.HunieBotCore));

        }
        private void SetAdditionalBindings()
        {
            _ninject.Unbind<IHunieUserPermissions>();
            _ninject.Bind<IHunieUserPermissions>().ToConstant(new ReadOnlyHunieUserPermissions(_userPermissions));
        }
        
        #region Discord Events

        private async void _incomingMessage(object sender, MessageEventArgs e)
        {
            await HandleEvent(e, CommandEvent.MessageReceived);
        }

        private async void _userUnbanned(object sender, UserEventArgs e)
        {
            await HandleEvent(e, CommandEvent.UserUnbanned);
        }

        private async void _userBanned(object sender, UserEventArgs e)
        {
            await HandleEvent(e, CommandEvent.UserBanned);
        }

        private async void _userLeft(object sender, UserEventArgs e)
        {
            await HandleEvent(e, CommandEvent.UserDeparted);
        }

        private async void _userJoined(object sender, UserEventArgs e)
        {
            await HandleEvent(e, CommandEvent.UserJoined);
        }

        private async void _joinedServer(object sender, ServerEventArgs e)
        {
            await HandleEvent(e, CommandEvent.JoinedServer);
        }

        private async void _departedServer(object sender, ServerEventArgs e)
        {
            await HandleEvent(e, CommandEvent.DepartedServer);
        }

        private async void _channelCreated(object sender, ChannelEventArgs e)
        {
            await HandleEvent(e, CommandEvent.ChannelCreated);
        }

        private async void _channelDestroyed(object sender, ChannelEventArgs e)
        {
            await HandleEvent(e, CommandEvent.ChannelDeleted);
        }

        private async void _channelUpdated(object sender, ChannelUpdatedEventArgs e)
        {
            await HandleEvent(e, CommandEvent.ChannelUpdated);
        }

        private async Task HandleEvent(EventArgs args, CommandEvent eventType)
        {
            IHunieEvent message;
            switch (eventType)
            {
                case CommandEvent.MessageReceived:
                    var mea = (args as MessageEventArgs);
                    if (mea == null) return;
                    message = mea.ToHunieMessage(_discordClientConnection);
                    if (mea.Channel.IsPrivate)
                    {
                        eventType |= CommandEvent.PrivateMessageReceived;
                    }
                    if (mea.Message.Text[0].Equals(Configuration.CommandCharacter))
                    {
                        eventType |= CommandEvent.CommandReceived;
                        message = new HunieCommand((IHunieMessage)message);
                    }
                    break;
                case CommandEvent.UserBanned:
                case CommandEvent.UserUnbanned:
                case CommandEvent.UserJoined:
                case CommandEvent.UserDeparted:
                    message = (args as UserEventArgs)?.ToHunieEvent(_discordClientConnection);
                    break;
                case CommandEvent.JoinedServer:
                case CommandEvent.DepartedServer:
                    message = (args as ServerEventArgs)?.ToHunieEvent(_discordClientConnection);
                    break;
                case CommandEvent.ChannelCreated:
                case CommandEvent.ChannelDeleted:
                    message = (args as ChannelEventArgs)?.ToHunieEvent(_discordClientConnection);
                    break;
                case CommandEvent.ChannelUpdated:
                    message = (args as ChannelUpdatedEventArgs)?.ToHunieEvent(_discordClientConnection);
                    break;
                default:
                    _logger.Info($"{eventType} not supported currently, sorry!");
                    return;
            }
            _logger.Trace($"{eventType} received. Passing on to bots...");
            foreach (var wrappedBot in _wrappers)
            {
                try
                {
                    await wrappedBot.HandleEvent(message, eventType);
                }
                catch (Exception e)
                {
                    _logger.Fatal($"Method Invocation for {wrappedBot.Name}", e);
                }
            }
        }

        #endregion

        /// <summary>
        ///     Wraps around a <see cref="HunieBotAttribute"/> flagged object and handles enumerating methods as appropriate.
        /// </summary>
        private sealed class HunieWrapper
        {
            private readonly DynamicMethodInjectorFactory _methodInjectorFactory = new DynamicMethodInjectorFactory();
            private readonly HunieBotAttribute _hba;
            private readonly IList<HunieMetaData> _commandMetaData;
            private readonly ILogging _logger;
            private readonly IKernel _kernel;
            private readonly IHunieUserPermissions _permissions;
            private readonly object _instance;



            /// <summary>
            ///     Gets the name of this <see cref="HunieWrapper"/>.
            /// </summary>
            public string Name => _hba.Name;

            /// <summary>
            ///     Gets the type of this <see cref="HunieWrapper"/>.
            /// </summary>
            public Type Type => _instance.GetType();



            /// <summary>
            ///     Creates a new instance of <see cref="HunieWrapper"/> to wrap around an instance of a <see cref="HunieBotAttribute"/> class.
            /// </summary>
            /// <param name="botAttribute">The <see cref="HunieBotAttribute"/> that supplies metadata about the instance being wrapped</param>
            /// <param name="bot">The instance of the bot being wrapped</param>
            /// <param name="logger">An instance of <see cref="ILogging"/> for reporting purposes</param>
            /// <param name="kernel">An instance of <see cref="IKernel"/> that we are going to (ab)use for method-based parameter injection</param>
            /// <param name="permissions"><see cref="IHunieUserPermissions"/> instance</param>
            public HunieWrapper(HunieBotAttribute botAttribute, object bot, ILogging logger, IKernel kernel, IHunieUserPermissions permissions)
            {
                _commandMetaData = new List<HunieMetaData>();
                _hba = botAttribute;
                _instance = bot;
                _kernel = kernel;
                _permissions = permissions;
                _logger = logger;

                // So, this instance is supposed to have some information.
                // We're going to use some reflection magic on this instance
                // to find all methods that are public that have the HandleEventAttribute
                // attribute on them.
                foreach (var method in _instance.GetType().GetMethods())
                {
                    var hea = method.GetCustomAttribute<HandleEventAttribute>(false);
                    if (hea == null) continue;
                    _commandMetaData.Add(new HunieMetaData(method, _methodInjectorFactory.Create(method), hea));
                }
            }

            /// <summary>
            ///     Propigates a <see cref="CommandEvent"/> to any applicable <see cref="HandleEventAttribute"/> methods.
            /// </summary>
            /// <param name="hEvent">The <see cref="IHunieEvent"/> to propigate</param>
            /// <param name="commandEvent">The <see cref="CommandEvent"/> that has occurred</param>
            internal async Task HandleEvent(IHunieEvent hEvent, CommandEvent commandEvent)
            {
                foreach (var methodData in (from c in _commandMetaData
                                            where (c.Attribute.Events & commandEvent) != 0 &&
                                            (hEvent.User != null && (_permissions[hEvent.User.Id] & c.Attribute.Permissions) == c.Attribute.Permissions)
                                            select c))
                {
                    await Task.Run(() =>
                    {
                        methodData.MethodInjector(_instance, BuildParameterArray(methodData.Parameters, hEvent, commandEvent));
                    });
                }
            }

            /// <summary>
            ///     Generates the appropriate object array to be passed into the method
            /// </summary>
            /// <param name="parameters">An array of <see cref="ParameterInfo"/> that are the parameters that must be supplied</param>
            /// <param name="hEvent"><see cref="IHunieEvent"/> instance that started all this</param>
            /// <returns>object array</returns>
            private object[] BuildParameterArray(ParameterInfo[] parameters, IHunieEvent hEvent, CommandEvent commandEvent)
            {
                // Optimization. We're going to assume a one-parameter array is the IHunieEvent.
                if(parameters.Length == 1)
                {
                    return new[] { hEvent };
                }

                // Alright. So, now we get to generate the object array we're gonna feed into the MethodInjector.
                // Pretty basic stuff here. Just walk the ParameterInfo[] and check each type. If it's a type we
                // know about in advance, we're gonna do something special with it. Otherwise, we'll fall back to
                // using Ninject to handle it. Who knows? We might know something special about it in DI one day.
                var parameterArray = new object[parameters.Length];
                for (int pCount = 0; pCount < parameterArray.Length; pCount++)
                {
                    var pType = parameters[pCount].ParameterType;
                    if(pType == typeof(CommandEvent))
                    {
                        parameterArray[pCount] = commandEvent;
                        continue;
                    }
                    if(pType == typeof(IHunieEvent))
                    {
                        parameterArray[pCount] = hEvent;
                        continue;
                    }
                    if(pType == typeof(IHunieMessage))
                    {
                        parameterArray[pCount] = hEvent as IHunieMessage;
                        continue;
                    }
                    if(pType == typeof(IHunieCommand))
                    {
                        parameterArray[pCount] = hEvent as IHunieCommand;
                        continue;
                    }
                    try
                    {
                        parameterArray[pCount] = _kernel.Get(pType);
                    }
                    catch (Exception e)
                    {
                        // Ewwww. This is... nasty, for sure.
                        _logger.Fatal($"Binding Error! Unable to resolve binding for {parameters[pCount].Name}.", e);
                        parameterArray[pCount] = null;
                    }
                }
                return parameterArray;
            }
            
        }

        /// <summary>
        ///     Property bag of related metadata.
        /// </summary>
        private sealed class HunieMetaData
        {
            private readonly Lazy<ParameterInfo[]> _lazyLoadParameters;
            public MethodInfo Method { get; }
            public MethodInjector MethodInjector { get; }
            public HandleEventAttribute Attribute { get; }
            public ParameterInfo[] Parameters { get; }


            public HunieMetaData(MethodInfo mi, MethodInjector mj, HandleEventAttribute hea)
            {
                Method = mi;
                MethodInjector = mj;
                Attribute = hea;
                Parameters = Method.GetParameters();
                ValidateParameters();
            }

            private void ValidateParameters()
            {
                ; // ToDo
            }

        }

    }

    /// <summary>
    ///     Extension methods 'cause I'm a bit lazy.
    /// </summary>
    internal static class Extensions
    {
        public static IHunieMessage ToHunieMessage(this MessageEventArgs args, DiscordClient c)
        {
            return new HunieMessage(args.Channel, args.Server, args.User, c, args.Message);
        }
        public static IHunieEvent ToHunieEvent(this UserEventArgs args, DiscordClient c)
        {
            return new HunieEvent(null, args.Server, args.User, c);
        }
        public static IHunieEvent ToHunieEvent(this ServerEventArgs args, DiscordClient c)
        {
            return new HunieEvent(null, args.Server, null, c);
        }
        public static IHunieEvent ToHunieEvent(this ChannelEventArgs args, DiscordClient c)
        {
            return new HunieEvent(args.Channel, args.Server, null, c);
        }
        public static IHunieEvent ToHunieEvent(this ChannelUpdatedEventArgs args, DiscordClient c)
        {
            return new HunieEvent(args.After, args.Server, null, c);
        }
    }

}