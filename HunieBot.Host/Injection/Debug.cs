using HunieBot.Host.Interfaces;
using Ninject;
using Ninject.Modules;

namespace HunieBot.Host.Injection
{

    /// <summary>
    ///     Loads up the debug <see cref="NinjectModule"/>.
    /// </summary>
    public sealed class Debug : NinjectModule
    {

        /// <summary>
        ///     Sets up dependency injection for DEBUG builds.
        /// </summary>
        public override void Load()
        {
            Bind<ILogging>().ToMethod(c => new Implementations.DebugLogger());
            Unbind<IKernel>();
            Bind<IKernel>().ToMethod(c => new StandardKernel(new Debug()));
        }
    }
}
