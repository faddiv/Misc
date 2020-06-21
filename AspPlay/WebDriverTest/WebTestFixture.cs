using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.TestHost;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Threading;
using WinAuth;
using WinAuth.Controllers;
using Microsoft.Extensions.DependencyInjection;
using WinAuth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace WebDriverTest
{
    public class WebTestFixture : IDisposable
    {
        private readonly IWebHost _host;
        private readonly SqliteConnection _connection;

        public WebTestFixture()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            _connection = new SqliteConnection(connectionString);
            var root = WebHostDefaults.ContentRootKey;
            var webHostBuilder = WebHost.CreateDefaultBuilder()
                .UseStartup(typeof(Startup));
            webHostBuilder.UseSolutionRelativeContentRoot("WinAuth");
            webHostBuilder.UseEnvironment("Development");
            webHostBuilder.UseHttpSys(options =>
            {
                options.Authentication.Schemes = AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                options.Authentication.AllowAnonymous = false;
                options.UrlPrefixes.Add("http://localhost:44310");
            });
            webHostBuilder.ConfigureTestServices(services =>
            {
                var mock = new Mock<IActiveDirectoryService>();
                mock.Setup(e => e.LoadData(It.IsAny<string>()))
                .Returns(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Key", "Value")
                });
                services.AddSingleton(typeof(IActiveDirectoryService), mock.Object);
                services.AddEntityFrameworkSqlite()
                .AddDbContext<Entities>(options =>
                {
                    options.UseSqlite(_connection);
                });
                Services = services.BuildServiceProvider();
            });
            _host = webHostBuilder.Build();
            _host.Start();

            BaseAddress = new Uri("http://localhost:44310");
            // Initialize the driver 
            var chromeOptions = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal,
            };
            Driver = new ChromeDriver(Environment.CurrentDirectory, chromeOptions);
            Driver.Manage().Window.Maximize();
        }

        public ChromeDriver Driver { get; }
        public Uri BaseAddress { get; }
        public ServiceProvider Services { get; private set; }

        public void Dispose()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception)
            {
            }
            try
            {
                _host.StopAsync();
                _host.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                _connection.Dispose();
            }
            catch (Exception)
            {

            }
        }
    }
}
