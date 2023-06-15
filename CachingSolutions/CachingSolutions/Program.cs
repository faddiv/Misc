// See https://aka.ms/new-console-template for more information
using CachingSolutions;
using LazyCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
var build = new ServiceCollection();
build.AddMemoryCache();
build.AddDistributedMemoryCache();
build.AddSingleton<LazyCache.IAppCache, LazyCache.CachingService>();
build.AddSingleton<ICustomCache, CustomCache>();
var sp = build.BuildServiceProvider();
var mc = sp.GetService<IMemoryCache>();
var dc = sp.GetService<IDistributedCache>();
var lc = sp.GetService<IAppCache>();
var cc = sp.GetService<ICustomCache>();
