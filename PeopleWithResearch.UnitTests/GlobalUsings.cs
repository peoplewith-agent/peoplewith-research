// Global usings for PeopleWithResearch.UnitTests
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.ComponentModel;
global using System.Linq;
global using System.Threading.Tasks;

// MAUI shim types made globally available so linked production source compiles
global using Microsoft.Maui;          // MainThread
global using Microsoft.Maui.Devices;  // DeviceInfo
global using Microsoft.Maui.Storage;  // Preferences
global using Sentry;                   // SentrySdk
global using Microsoft.Maui.Controls;  // INavigation, ImageSource, Brush, Page, etc.
global using PeopleWithResearch;  // Models live in PeopleWithResearch namespace (no .Models sub-namespace)

