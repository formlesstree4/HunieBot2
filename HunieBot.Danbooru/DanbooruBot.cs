using HunieBot.Host.Attributes;
using HunieBot.Host.Enumerations;
using HunieBot.Host.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace HunieBot.Danbooru
{

    [HunieBot("BooruBot")]
    public class DanbooruBot
    {


        [HandleCommand(CommandEvent.AnyMessageReceived | CommandEvent.CommandReceived, UserPermissions.User, true, "booru")]
        public async Task Search()
        {








        }



    }
}
