﻿namespace TodoApi;

public static class AuthenticationExtensions
{
    public static WebApplicationBuilder AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication().AddBearerToken(AuthenticationConstants.BearerTokenScheme);

        return builder;
    }
}
