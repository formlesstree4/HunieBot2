using System;
using System.Collections.Generic;

namespace HunieBot.Host.Interfaces
{

    /// <summary>
    ///     Internal metadata that is presented on behalf of <see cref="HunieHost"/>
    /// </summary>
    internal interface IHunieHostMetaData
    {

        /// <summary>
        ///     Gets
        /// </summary>
        IReadOnlyCollection<HunieHost.HunieWrapper> Commands { get; }

    }
}
