
using System;

namespace ChainSawLeg.Application
{
    public interface IStartupResolver
    {
        Type Resolve();
    }
}

