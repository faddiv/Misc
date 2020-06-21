# AspPlay

In this project I tested some asp.net core (with .net core) functionality.

In the main project I used the following features:
1. Windows identity authentication.
2. Active Directory query.
3. Custom roles for Windows identity. (I replaced Windows identity with claims identity)
4. webpack build for multi page application. (Common and per-page js and ccss files.)

In the test project I used the following features:
1. Selenium testing with in process web server. This opens up bunch of nice stuff I can do.
2. Mocking out a service in the web app.
3. Configure test server so it uses windows identity.
4. Use in memory database in the web app.
