﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lyra.Core.Decentralize
{
    public interface ILyraGossip
    {
        Task<Guid> Join(string nickname);
        Task<Guid> Leave(string nickname);
        Task<bool> Message(ChatMsg msg);
        Task<string[]> GetMembers();
    }
}
