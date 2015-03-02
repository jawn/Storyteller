﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FubuCore;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Storyteller.Core.Engine;
using Storyteller.Core.Engine.Batching;
using Storyteller.Core.Messages;
using Storyteller.Core.Model;
using Storyteller.Core.Model.Persistence;
using Storyteller.Core.Remotes;
using Storyteller.Core.Remotes.Messaging;
using Storyteller.Core.Results;
using Storyteller.Core.Testing.Engine;
using StoryTeller.Samples;
using StoryTeller.Samples.Fixtures;

namespace Storyteller.Core.Testing
{
    [TestFixture, Explicit]
    public class record_data_for_client_testing
    {
        private GrammarSystem theSystem;


        [TestFixtureSetUp]
        public void SetUp()
        {
            theSystem = new GrammarSystem();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            theSystem.Dispose();
        }

        [Test]
        public void record_and_save_file()
        {
            var data = new ClientData();
            
            var library = TestingContext.Library;

            
            var hierarchy = TestingContext.Hierarchy;

            hierarchy.GetAllSpecs().Each(header =>
            {
                Debug.WriteLine("Running " + header.name);

                var spec = XmlReader.ReadFromFile(header.Filename);
                data.specs.Add(spec.Id, spec);

                using (var execution = theSystem.CreateContext())
                {
                    var observer = new RecordingObserver();
                    var context = new SpecContext(spec, observer, new StopConditions(), execution.Services);
                    var plan = spec.CreatePlan(library);
                    var executor = new SynchronousExecutor(context);

                    plan.AcceptVisitor(executor);

                    observer.SpecExecutionFinished(header, context.Counts);

                    data.results.Add(spec.Id, observer.Messages.ToArray());
                }
            });



            data.hierarchy = hierarchy;
            data.fixtures = library.Models.Where(x => x.implementation.Contains("StoryTeller.Samples")).ToArray();

            var json = JsonSerialization.ToIndentedJson(data);

            new FileSystem().WriteStringToFile("all-spec-data.js", "module.exports = " + json);
            Debug.WriteLine("Wrote file to all-spec-data.js");
        }
    }

    public class RecordingObserver : IResultObserver
    {
        public readonly IList<ClientMessage> Messages = new List<ClientMessage>(); 

        public void Handle<T>(T message) where T : IResultMessage
        {
            Messages.Add(message.As<ClientMessage>());
        }

        public void SpecExecutionFinished(SpecNode node, Counts counts)
        {
            Messages.Add(new SpecExecutionCompleted(node.id, counts, 0));
        }
    }

    public class ClientData
    {
        public FixtureModel[] fixtures;
        public readonly Dictionary<string, Specification> specs = new Dictionary<string, Specification>();
        public Suite hierarchy;

        public readonly Dictionary<string, ClientMessage[]> results = new Dictionary<string, ClientMessage[]>(); 
    }
}