using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Linq;
using WinAuth.Data;
using Xunit;

namespace WebDriverTest
{
    [Collection("WebTest")]
    public class WebDriverTest
    {
        private readonly ChromeDriver _driver;
        private readonly WebTestFixture _webTest;

        public string WindowsUser => $"{Environment.UserDomainName}\\{Environment.UserName}";

        public WebDriverTest(WebTestFixture webTest)
        {
            _driver = webTest.Driver;
            _webTest = webTest;
        }

        [Fact]
        public void VerifyPageTitle()
        {
            // Replace with your own test logic
            _driver.Navigate().GoToUrl(_webTest.BaseAddress);
            Assert.Equal("Home Page - WinAuth", _driver.Title);
        }

        [Fact]
        public void WindowsUserLoggedIn()
        {
            _driver.Navigate().GoToUrl(_webTest.BaseAddress);
            var element = _driver.FindElement(By.CssSelector(".nav.navbar-text.navbar-right"));
            Assert.NotNull(element);
            string text = element.Text;
            Assert.Equal($"Hello, {WindowsUser}!", text);
        }

        [Fact]
        public void ActiveDirectoryServiceIsMocked()
        {
            _driver.Navigate().GoToUrl(_webTest.BaseAddress);
            var element = _driver.FindElement(By.CssSelector("dl"));
            Assert.NotNull(element);
            string text = element.Text;
            Assert.Equal("Key\r\nValue", text);
        }

        [Fact]
        public void AdminCanGoAbout()
        {
            using (var scope = _webTest.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<Entities>();
                ctx.Users.Add(new User
                {
                    UserName = WindowsUser
                });
                ctx.SaveChanges();
            }
            _driver.Navigate().GoToUrl(new Uri(_webTest.BaseAddress, "/Home/About"));
            var element = _driver.FindElementById("admin");
            Assert.Equal("Admin Page", element.Text);
        }

        [Fact]
        public void NonAdminCantGoAbout()
        {
            using (var scope = _webTest.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<Entities>();
                ctx.Users.RemoveRange(ctx.Users.Where(u => u.UserName == WindowsUser));
                ctx.SaveChanges();
            }
            _driver.Navigate().GoToUrl(new Uri(_webTest.BaseAddress, "/Home/About"));
            var element = _driver.FindElementById("main-frame-error");
            Assert.Contains("HTTP ERROR 403", element.Text);
        }
    }
}
