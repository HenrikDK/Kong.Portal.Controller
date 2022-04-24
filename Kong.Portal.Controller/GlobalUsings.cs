global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using Flurl;
global using Flurl.Http;
global using Flurl.Http.Configuration;
global using k8s;
global using Lamar;
global using Lamar.Microsoft.DependencyInjection;
global using MoreLinq.Extensions;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using Prometheus;
global using Serilog;
global using Serilog.Formatting.Json;