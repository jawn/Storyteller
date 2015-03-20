using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FubuCore;
using Storyteller.Core;
using Storyteller.Core.Conversion;
using Storyteller.Core.Engine;

namespace StoryTeller.Samples
{
    public class GrammarSystem : ISystem
    {
        public readonly InMemoryServiceLocator Services = new InMemoryServiceLocator();

        public IExecutionContext CreateContext()
        {
            if (Project.CurrentProfile == "blowup")
            {
                throw new Exception("I blew up trying to create an execution context");
            }

            return new NulloSystem.SimpleExecutionContext(Services);
        }

        public Task Recycle()
        {
            return Task.FromResult("recycled");
        }

        public Task Warmup()
        {
            return Task.FromResult("warmed up");
        }

        public void Dispose()
        {
        }

        public IEnumerable<IConversionProvider> ConversionProviders()
        {
            return new IConversionProvider[0];
        }
    }



}