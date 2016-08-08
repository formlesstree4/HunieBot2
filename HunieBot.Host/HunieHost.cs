using Discord;
using HunieBot.Host.Attributes;
using HunieBot.Host.Database;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Injection;
using HunieBot.Host.Injection.Implementations.Permissions;
using HunieBot.Host.Interfaces;
using Ninject;
using Ninject.Injection;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly static string DatabaseFile = Path.Combine(HunieBotDataFolder, "huniebot.sqlite");

        // private readonly static string ConfigurationFile = Path.Combine(HunieBotDataFolder, "configuration.json");
        // private readonly static string UserPermissionsFile = Path.Combine(HunieBotDataFolder, "userpermissions.json");
        // private readonly static string CommandPermissionsFile = Path.Combine(HunieBotDataFolder, "commandpermissions.json");


        private readonly IHunieCommandPermissions _commandPermissions;
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

            // Let's make sure our appdata directory exists!
            if (!Directory.Exists(HunieBotDataFolder)) Directory.CreateDirectory(HunieBotDataFolder);

            _discordClientConnection = new DiscordClient(dc);
            _ninject = new StandardKernel(_loadedModule);
            _ninject.Bind<HunieConnectionManager>().ToConstant(new HunieConnectionManager(DatabaseFile));
            _ninject.Bind<IHunieCommandPermissions>().ToConstant(new HunieCommandPermissions(_ninject.Get<HunieConnectionManager>()));
            _ninject.Bind<IHunieUserPermissions>().ToConstant(new HunieUserPermissions(_ninject.Get<HunieConnectionManager>()));
            _ninject.Bind<DiscordClient>().ToConstant(_discordClientConnection);
            _logger = _ninject.Get<ILogging>();
            _userPermissions = _ninject.Get<IHunieUserPermissions>();
            _commandPermissions = _ninject.Get<IHunieCommandPermissions>();
            _wrappers = new List<HunieWrapper>();
            RegisterEvents();
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
            _discordClientConnection.SetGame(Configuration.Game);
        }

        /// <summary>
        ///     Asynchronously stops <see cref="HunieHost"/>
        /// </summary>
        /// <returns>A promise to stop <see cref="HunieHost"/></returns>
        public async Task Stop()
        {
            await _discordClientConnection.Disconnect();
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
            var wrapper = new HunieWrapper(hea, instance, _ninject.Get<ILogging>(), _ninject.Get<IKernel>(), _ninject.Get<IHunieUserPermissions>(), _ninject.Get<IHunieCommandPermissions>());
            _wrappers.Add(wrapper);
        }

        /// <summary>
        ///     Disposes of <see cref="HunieHost"/>
        /// </summary>
        public void Dispose()
        {
            ReleaseEvents();
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
            // _ninject.Bind<IHunieUserPermissions>().ToConstant(_userPermissions);

            // Let's create our internal instances.
            RegisterHunieBotType(typeof(Internal.HunieBotCore));

        }
        private void SetAdditionalBindings()
        {
            _ninject.Unbind<IHunieUserPermissions>(); // Initially, we bound this to a read/write version. We're going to expose a readonly version now.
            _ninject.Bind<IHunieUserPermissions>().ToConstant(new ReadOnlyHunieUserPermissions(_userPermissions));
            _ninject.Bind<IHunieHostMetaData>().ToConstant(new HunieHostMetaData(((List<HunieWrapper>)_wrappers).AsReadOnly()));
        }
        private void RegisterEvents()
        {
            _discordClientConnection.Ready += _ready;
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
        private void ReleaseEvents()
        {
            _discordClientConnection.Ready -= _ready;
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


        #region Discord Events

        private async void _ready(object sender, EventArgs e)
        {
            await HandleEvent(e, CommandEvent.Connected);
        }

        private async void _incomingMessage(object sender, MessageEventArgs e)
        {
            if (e.Channel.IsPrivate)
            {
                await HandleEvent(e, CommandEvent.PrivateMessageReceived);
            }
            else
            {
                await HandleEvent(e, CommandEvent.MessageReceived);
            }
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
                case CommandEvent.Connected:
                    message = new HunieEvent(null, null, null, _discordClientConnection);
                    break;
                case CommandEvent.PrivateMessageReceived:
                case CommandEvent.MessageReceived:
                    var mea = (args as MessageEventArgs);
                    if (mea == null) return;
                    message = mea.ToHunieMessage(_discordClientConnection);
                    if (!string.IsNullOrWhiteSpace(mea.Message.Text) && mea.Message.Text[0].Equals(Configuration.CommandCharacter))
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
        ///     Metadata about <see cref="HunieHost"/>.
        /// </summary>
        private sealed class HunieHostMetaData : IHunieHostMetaData
        {

            /// <summary>
            ///     Gets a collection of <see cref="HunieWrapper"/> instances.
            /// </summary>
            public IReadOnlyCollection<HunieWrapper> Commands { get; }



            /// <summary>
            ///     Creates a new instance of <see cref="HunieHostMetaData"/>.
            /// </summary>
            /// <param name="cmdWrappers">A read only collection of <see cref="HunieWrapper"/></param>
            public HunieHostMetaData(IReadOnlyCollection<HunieWrapper> cmdWrappers)
            {
                Commands = cmdWrappers;
            }

        }

        /// <summary>
        ///     Wraps around a <see cref="HunieBotAttribute"/> flagged object and handles enumerating methods as appropriate.
        /// </summary>
        internal sealed class HunieWrapper
        {
            private readonly DynamicMethodInjectorFactory _methodInjectorFactory = new DynamicMethodInjectorFactory();
            private readonly HunieBotAttribute _hba;
            private readonly IList<HunieEventMetaData> _eventMetaData;
            private readonly IList<HunieCommandMetaData> _commandMetaData;
            private readonly ILogging _logger;
            private readonly IKernel _kernel;
            private readonly IHunieCommandPermissions _commandPermissions;
            private readonly IHunieUserPermissions _userPermissions;
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
            ///     Returns the commands that this <see cref="HunieWrapper"/> supports.
            /// </summary>
            public IEnumerable<string> Commands
            {
                get
                {
                    foreach (var hcmd in _commandMetaData)
                    {
                        foreach (var cmd in hcmd.Attribute.Commands)
                        {
                            yield return cmd;
                        }
                    }
                }
            }


            internal IReadOnlyCollection<HunieCommandMetaData> CommandMetadata => ((List<HunieCommandMetaData>)_commandMetaData).AsReadOnly();


            /// <summary>
            ///     Creates a new instance of <see cref="HunieWrapper"/> to wrap around an instance of a <see cref="HunieBotAttribute"/> class.
            /// </summary>
            /// <param name="botAttribute">The <see cref="HunieBotAttribute"/> that supplies metadata about the instance being wrapped</param>
            /// <param name="bot">The instance of the bot being wrapped</param>
            /// <param name="logger">An instance of <see cref="ILogging"/> for reporting purposes</param>
            /// <param name="kernel">An instance of <see cref="IKernel"/> that we are going to (ab)use for method-based parameter injection</param>
            /// <param name="userPermissions"><see cref="IHunieUserPermissions"/> instance</param>
            public HunieWrapper(HunieBotAttribute botAttribute, object bot, ILogging logger, IKernel kernel, IHunieUserPermissions userPermissions, IHunieCommandPermissions commandPermissions)
            {
                _eventMetaData = new List<HunieEventMetaData>();
                _commandMetaData = new List<HunieCommandMetaData>();
                _hba = botAttribute;
                _instance = bot;
                _kernel = kernel;
                _userPermissions = userPermissions;
                _commandPermissions = commandPermissions;
                _logger = logger;

                // So, this instance is supposed to have some information.
                // We're going to use some reflection magic on this instance
                // to find all methods that are public that have the HandleEventAttribute
                // attribute on them.
                foreach (var method in _instance.GetType().GetMethods())
                {
                    var hea = method.GetCustomAttribute<HandleEventAttribute>(false);
                    var hca = method.GetCustomAttribute<HandleCommandAttribute>(false);
                    if (hea != null && hca != null)
                    {
                        throw new InvalidOperationException($"You may not have {nameof(HandleEventAttribute)} and {nameof(HandleCommandAttribute)} on the same method ({method.Name}).");
                    }
                    if (hea == null && hca == null) continue; // This method does not have attributes we care about.

                    if(hea != null)
                    {
                        _eventMetaData.Add(new HunieEventMetaData(method, _methodInjectorFactory.Create(method), hea));
                        continue;
                    }
                    if(hca != null)
                    {
                        var desc = method.GetCustomAttribute<DescriptionAttribute>();
                        _commandMetaData.Add(new HunieCommandMetaData(method, _methodInjectorFactory.Create(method), hca, desc));
                        continue;
                    }
                }
            }



            /// <summary>
            ///     Propigates a <see cref="CommandEvent"/> to any applicable <see cref="HandleEventAttribute"/> methods.
            /// </summary>
            /// <param name="hEvent">The <see cref="IHunieEvent"/> to propigate</param>
            /// <param name="commandEvent">The <see cref="CommandEvent"/> that has occurred</param>
            internal async Task HandleEvent(IHunieEvent hEvent, CommandEvent commandEvent)
            {
                // So, basically, we're going to check CommandEvent to see if it is a CommandReceived type.
                // If it is, we're going to iterate across the command list instead of the
                // event list. It makes pretty logical sense.
                if ((commandEvent & CommandEvent.CommandReceived) != 0)
                {
                    await HandleAsCommand(hEvent, commandEvent);
                }
                else
                {
                    await HandleAsEvent(hEvent, commandEvent);
                }
            }

            /// <summary>
            ///     Passes <see cref="IHunieEvent"/> through to <see cref="HunieEventMetaData"/> instances for invocation.
            /// </summary>
            /// <param name="hEvent"><see cref="IHunieEvent"/></param>
            /// <param name="commandEvent"><see cref="CommandEvent"/> that invoked <see cref="HandleAsEvent(IHunieEvent, CommandEvent)"/></param>
            /// <returns>A promise to handle the event</returns>
            private async Task HandleAsEvent(IHunieEvent hEvent, CommandEvent commandEvent)
            {
                foreach (var methodData in (from c in _eventMetaData
                                            where (c.Attribute.Events & commandEvent) == commandEvent
                                            select c))
                {
                    await Task.Run(() =>
                    {
                        methodData.MethodInjector(_instance, BuildParameterArray(methodData.Parameters, hEvent, commandEvent));
                    });
                }
            }

            /// <summary>
            ///     Passes <see cref="IHunieEvent"/> through to <see cref="HunieCommandMetaData"/> instances for invocation.
            /// </summary>
            /// <param name="hEvent"><see cref="IHunieEvent"/></param>
            /// <param name="commandEvent"><see cref="CommandEvent"/> that invoked <see cref="HandleAsCommand(IHunieEvent, CommandEvent)"/></param>
            /// <returns>A promise to handle the command</returns>
            private async Task HandleAsCommand(IHunieEvent hEvent, CommandEvent commandEvent)
            {
                var cmd = hEvent as IHunieCommand;

                /*
                    for each item in the command metadata collection, we are going
                    to select out the ones that meet thhe following criteria:
                    1) Make sure that we pull a command that actually handles the Command text that was submitted.
                    1) The event must be a supported command event.
                        Example: If the event is a PrivateMessage, the only commands
                        that accept private messages are going to be pulled.
                    3) Make sure that the User can execute this event.
                    4) Make sure that the Channel this command was sent from can execute this command:
                        A) If the channel is private, the command can always be executed (so long as the command is configured to receive private messages)
                        B) If the command is opted in to using permission filtering, check the internal permissions class to ensure that this command may
                        execute for the channel+server combination.
                */
                foreach (var methodData in (from c in _commandMetaData
                                            where
                 DoesCommandExistInMetaData(cmd, c) &&
                 DoesMetaDataSupportCommandEvent(commandEvent, c) &&
                 DoesUserHavePermissionToExecuteCommand(hEvent, c) &&
                 DoesCommandHavePermissionToExecute(hEvent, c) select c))
                {
                    await Task.Run(() =>
                    {
                        methodData.MethodInjector(_instance, BuildParameterArray(methodData.Parameters, cmd, commandEvent));
                    });
                }
            }

            private bool DoesCommandHavePermissionToExecute(IHunieEvent hEvent, HunieCommandMetaData c)
            {
                return
                (
                    hEvent.Channel.IsPrivate ||
                    !c.Attribute.IsPermissionsDriven ||
                    _commandPermissions[hEvent.Server.Id, hEvent.Channel.Id, c.Attribute.Commands]
                );
            }

            private bool DoesUserHavePermissionToExecuteCommand(IHunieEvent hEvent, HunieCommandMetaData c)
            {
                return hEvent.Channel.IsPrivate || ((_userPermissions[hEvent.Server.Id, hEvent.User.Id] & c.Attribute.Permissions) == c.Attribute.Permissions);
            }

            private bool DoesMetaDataSupportCommandEvent(CommandEvent commandEvent, HunieCommandMetaData c)
            {
                return (c.Attribute.Events & commandEvent) == commandEvent;
            }

            private bool DoesCommandExistInMetaData(IHunieCommand cmd, HunieCommandMetaData c)
            {
                return (c.Attribute.Commands.Contains(cmd.Command, StringComparer.OrdinalIgnoreCase));
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
                    var p = parameters[pCount];
                    var pType = p.ParameterType;
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
        ///     Property bag of related metadata for dealing with <see cref="HandleEventAttribute"/>.
        /// </summary>
        internal sealed class HunieEventMetaData
        {
            public MethodInfo Method { get; }
            public MethodInjector MethodInjector { get; }
            public HandleEventAttribute Attribute { get; }
            public ParameterInfo[] Parameters { get; }


            public HunieEventMetaData(MethodInfo mi, MethodInjector mj, HandleEventAttribute hea)
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

        /// <summary>
        ///     Property bag of related metadata for dealing with <see cref="HandleCommandAttribute"/>.
        /// </summary>
        internal sealed class HunieCommandMetaData
        {
            public MethodInfo Method { get; }
            public MethodInjector MethodInjector { get; }
            public HandleCommandAttribute Attribute { get; }
            public DescriptionAttribute Description { get; }
            public ParameterInfo[] Parameters { get; }


            public HunieCommandMetaData(MethodInfo mi, MethodInjector mj, HandleCommandAttribute hca, DescriptionAttribute desc)
            {
                Method = mi;
                MethodInjector = mj;
                Attribute = hca;
                Parameters = Method.GetParameters();
                Description = desc;
                ValidateParameters();
            }

            private void ValidateParameters()
            {
                foreach (var pType in Parameters.Select(c => c.ParameterType))
                {
                    if (pType == typeof(IHunieCommand)) return;
                }
                throw new ArgumentException();
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
        public static IEnumerable<string> ParseParameters(this string p)
        {
            return p.Split('"')
                .Select((element, index) => index % 2 == 0
                    ? element.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    : new[] { element })
                .SelectMany(element => element).ToList();
        }
    }

}