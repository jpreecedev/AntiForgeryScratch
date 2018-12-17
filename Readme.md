# Verify CSRF token with .NET Core

A simple repo that demonstrates how to verify that a server request is legitimate and originated from the user, mitigating cross site request forgery attacks.

## How to

In summary:

- Add the `AntiForgery` service to the pipeline
- Add `[ValidateAntiForgeryToken]` attribute to at least all `POST` requests
- Send cookie as a header when making AJAX requests

### Add AntiForgery to service pipeline

Open **Startup.cs** and add the following;

`ConfigureServices` method:

```csharp
services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
```

`Configure` method:
Update signature to inject `IAntiForgery` service;

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, IAntiforgery antiforgery)
```

Then call the `UseXsrf` extension method passing in the `AntiForgery` service (we will define this extension method next).

```csharp
app.UseXsrf(antiforgery);
```

### `UseXsrf` extension method

Create a new file called **AntiForgeryExtensions** and add the following;

```csharp
public static class CsrfToken
{
    public static IApplicationBuilder UseXsrf(this IApplicationBuilder app, IAntiforgery antiforgery)
    {
        return app.Use((context, next) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions {HttpOnly = false});
            return next.Invoke();
        });
    }
}
```

### Add `[ValidateAntiForgeryToken]` to your `POST` requests

Should be fairly self explanatory

### Send cookie as a header when making AJAX requests

In your JavaScript code, when making AJAX requests.

Use the following code to easily read cookies;

```javascript
static getCookie(name) {
    let value = '; ' + document.cookie;
    let parts = value.split('; ' + name + '=');
    if (parts.length === 2) {
        return parts.pop().split(';').shift()
    }
}
```

Then append `X-XSRF-TOKEN` header to each (usually) `POST` request;

```javascript
fetch("api/SampleData/WeatherForecasts", {
  headers: {
    "X-XSRF-TOKEN": FetchData.getCookie("XSRF-TOKEN")
  }
})
  .then(response => response.json())
  .then(data => {
    this.setState({ forecasts: data, loading: false });
  });
```

Invalid requests should now be blocked.
