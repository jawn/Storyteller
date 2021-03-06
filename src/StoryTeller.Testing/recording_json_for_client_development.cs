﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Baseline;
using StoryTeller.Engine;
using StoryTeller.Messages;
using StoryTeller.Model;
using StoryTeller.Model.Persistence;
using StoryTeller.Model.Persistence.Markdown;
using StoryTeller.Remotes;
using StoryTeller.Remotes.Messaging;
using ST.CommandLine;

namespace StoryTeller.Testing
{
    public class recording_json_for_client_development
    {
        private readonly string samplesFolder = TestingContext.FindParallelDirectory("Storyteller.Samples");

        //[Fact]
        public void record_specification_json()
        {
            var hierarchy = TestingContext.Hierarchy;
            var dictionary = new Dictionary<string, Specification>();

            hierarchy.GetAllSpecs().Each(x =>
            {
                var spec = MarkdownReader.ReadFromFile(x.Filename);
                dictionary.Add(x.id, spec);
            });

            var json = JsonSerialization.ToIndentedJson(dictionary);

            new FileSystem().WriteStringToFile("Specifications.js", "module.exports = " + json);
        }

        //[Fact]
        public void write_initial_model()
        {
            // You need to compile everything before trying to use this
            var input = new ProjectInput(EngineMode.Batch)
            {
                Path =
                    samplesFolder,
                ProfileFlag = "Safari"
            };

            using (var controller = input.BuildRemoteController())
            {
                controller.Start().Wait(30.Seconds());

                var hierarchy = HierarchyLoader.ReadHierarchy(input.Path.AppendPath("Specs"));
                var request = new BatchRunRequest
                {
                    SpecPath = input.SpecPath
                };

                var response = controller.Send(request).AndWaitFor<BatchRunResponse>();


                var cache = new ResultsCache();
                response.Result.records.Each(
                    x =>
                    {
                        var completed = new SpecExecutionCompleted(x.specification.id, x.results, x.specification);
                        cache.Store(completed);
                    });

                response.Result.fixtures = controller.LatestSystemRecycled.fixtures;


                var hierarchyLoaded = new HierarchyLoaded(hierarchy, cache);

                var initialization = new InitialModel(controller.LatestSystemRecycled, hierarchyLoaded)
                {
                    wsAddress = "ws://localhost:" + 8200
                };

                writeResponse(response.Result);
                //writeInitialization(initialization);
            }
        }

        private void writeResponse(BatchRunResponse response)
        {
            var json = JsonSerialization.ToIndentedJson(response);

            var path = TestingContext.FindClientFolder().AppendPath("batch-run-response-data.js");

            new FileSystem().WriteStringToFile(path, "module.exports = " + json);
        }

        private static void writeInitialization(InitialModel initialization)
        {
            var json = JsonSerialization.ToIndentedJson(initialization);

            var path = TestingContext.FindClientFolder().AppendPath("initialization.js");

            new FileSystem().WriteStringToFile(path, "module.exports = " + json);
        }

        //[Fact]
        public void write_the_table5_spec()
        {
            var hierarchy = TestingContext.Hierarchy;
            var spec = hierarchy.ToHierarchy().Specifications["table5"];
            var specification = MarkdownReader.ReadFromFile(spec.Filename);

            var json = JsonSerialization.ToIndentedJson(specification);

            Debug.WriteLine(json);
        }
    }
}