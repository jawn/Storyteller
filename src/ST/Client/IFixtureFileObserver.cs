﻿using System;

namespace ST.Client
{
    public interface IFixtureFileObserver : IDisposable
    {
        void Changed(string file);
        void Added(string file);
        void Deleted(string file);
    }
}