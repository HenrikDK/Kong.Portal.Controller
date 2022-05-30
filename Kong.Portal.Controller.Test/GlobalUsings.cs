global using System;
global using System.Collections.Generic;
global using System.Net.Http;
global using System.IO;
global using System.Threading;

global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using FluentAssertions;
global using Flurl;
global using Flurl.Http;
global using Flurl.Http.Configuration;
global using Lamar;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using Newtonsoft.Json.Linq;
global using NUnit.Framework;
global using NSubstitute;
global using TestStack.BDDfy.Configuration;
global using TestStack.BDDfy.Reporters.Html;
global using TestStack.BDDfy;

