global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Dynamic;
global using System.IO;
global using System.IO.Compression;
global using System.Linq;
global using System.Net.Http;
global using System.Runtime.InteropServices;
global using System.Text;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

global using Flurl;
global using Flurl.Http;
global using Flurl.Http.Configuration;
global using Lamar.Microsoft.DependencyInjection;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;
global using Prometheus;
global using Swashbuckle.AspNetCore.SwaggerUI;
global using Serilog;
global using Serilog.Formatting.Json;
global using YamlDotNet.Serialization;
