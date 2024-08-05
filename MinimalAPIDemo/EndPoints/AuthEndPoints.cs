using System.Net;
using AutoMapper;
using Azure;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.AuthDto;
using MinimalAPIDemo.Models.DTO;
using MinimalAPIDemo.Repository.IRepository;
using MinimalAPIDemo.Shared;

namespace MinimalAPIDemo.EndPoints
{
    public static class AuthEndPoints
    {
        public static void ConfigureAuthEndPoints(this WebApplication app)
        {
            app.MapPost("api/login",LoginAsync)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.OK)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.BadRequest)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Unauthorized)
                .Accepts<LoginRequestDto>("application/json")
                .WithName("Login");

            app.MapPost("api/register", RegisterAsync)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Created)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.BadRequest)
                .Accepts<RegistrationRequestDto>("application/json")
                .WithName("Register");

            app.MapPost("api/refreshToken", RefreshTokenAsync)
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.OK)
                .Produces<APIResponse>((int)HttpStatusCode.BadRequest)
                .Accepts<TokenDto>("application/json")
                .WithName("RefreshToken");

            app.MapPost("api/revokeRefreshToken", RevokeRefreshTokenAsync)
                .Produces<APIResponse>()
                .Produces<APIResponse>(statusCode: (int)HttpStatusCode.OK)
                .Produces<APIResponse>((int)HttpStatusCode.BadRequest)
                .Accepts<TokenDto>("application/json")
                .WithName("RevokeRefreshToken");

        }

        private static async Task<IResult> LoginAsync(IAuthRepository authRepository, IMapper mapper,
            [FromBody] LoginRequestDto loginRequestDto, IValidator<LoginRequestDto> validator, ILogger<Program> logger)
        {
            logger.Log(LogLevel.Information, "Login user");

            var response = new APIResponse(
                false,
                StatusCode: HttpStatusCode.BadRequest,
                ErrorMessages: []
            );

            // Validate the input model
            var validationResult = await validator.ValidateAsync(loginRequestDto);

            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                response = response with { StatusCode = HttpStatusCode.BadRequest, ErrorMessages = errors };
                return Results.BadRequest(response);
            }

            // Perform login
            var loginResponse = await authRepository.LoginAsync(loginRequestDto);

            if (loginResponse?.AccessToken != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
            {
                response = response with { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = loginResponse };
                return Results.Ok(response);
            }

            response = response with { StatusCode = HttpStatusCode.BadRequest, IsSuccess = false, ErrorMessages = ["Username or password is incorrect."] };
            return Results.BadRequest(response);
        }


        private static async Task<IResult> RegisterAsync(IAuthRepository authRepository, IMapper mapper,
            [FromBody] RegistrationRequestDto registrationRequestDto, ILogger<Program> logger)
        {
            logger.Log(LogLevel.Information, "Register user");
            var response = new APIResponse(
                false,
                StatusCode: HttpStatusCode.BadRequest,
                ErrorMessages: []
            );
            try
            {
                var ifUserNameUnique = authRepository.IsUniqueUser(registrationRequestDto.Email);
                if (ifUserNameUnique)
                {
                    var registerResponse = await authRepository.RegisterAsync(registrationRequestDto);
                    if (registerResponse is not null)
                    {
                        response = response with { StatusCode = HttpStatusCode.OK };
                        response = response with { Result = registerResponse };
                        return Results.Ok(response);
                    }

                    response = response with { StatusCode = HttpStatusCode.BadRequest };
                    response = response with { IsSuccess = false };
                    response = response with { ErrorMessages = ["Error while registering."] };
                    return Results.BadRequest(response);
                }

                response = response with { StatusCode = HttpStatusCode.BadRequest };
                response = response with { IsSuccess = false };
                response = response with { ErrorMessages = ["Username already exists."] };
                return Results.BadRequest(response);
            }
            catch (Exception ex)
            {
                response = response with { StatusCode = HttpStatusCode.InternalServerError };
                response = response with { IsSuccess = false };
                response = response with { ErrorMessages = ["Internal server error."] };
                return Results.InternalServerError(response);
            }
        }

        private static async Task<IResult> RefreshTokenAsync(
            IAuthRepository authRepository,
            IMapper mapper,
            TokenDto tokenDto,
            ILogger<Program> logger,
            IValidator<TokenDto> validator)
        {
            logger.Log(LogLevel.Information, "Refresh Token");

            // Validate the model
            var validationResult = await validator.ValidateAsync(tokenDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = new APIResponse(
                    false,
                    StatusCode: HttpStatusCode.BadRequest,
                    ErrorMessages: errors
                );
                return Results.BadRequest(response);
            }

            // Proceed with token refresh if validation passes
            var tokenDtoResponse = await authRepository.RefreshAccessToken(tokenDto);
            if (tokenDtoResponse is null || string.IsNullOrEmpty(tokenDtoResponse.AccessToken))
            {
                var errorResponse = new APIResponse(
                    false,
                    StatusCode: HttpStatusCode.BadRequest,
                    ErrorMessages: ["Token Invalid."]
                );
                return Results.BadRequest(errorResponse);
            }

            var successResponse = new APIResponse(
                true,
                StatusCode: HttpStatusCode.OK,
                Result: tokenDtoResponse
            );
            return Results.Ok(successResponse);
        }


        private static async Task<IResult> RevokeRefreshTokenAsync(
            IAuthRepository authRepository,
            IMapper mapper,
            TokenDto tokenDto,
            ILogger<Program> logger,
            IValidator<TokenDto> validator)
        {
            logger.Log(LogLevel.Information, "Revoke Refresh Token");

            // Validate the model
            var validationResult = await validator.ValidateAsync(tokenDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                var response = new APIResponse(
                    false,
                    StatusCode: HttpStatusCode.BadRequest,
                    ErrorMessages: errors
                );
                return Results.BadRequest(response);
            }

            // Proceed with the actual operation if validation passes
            await authRepository.RevokeRefreshToken(tokenDto);
            var successResponse = new APIResponse(
                true,
                StatusCode: HttpStatusCode.OK,
                ErrorMessages: []
            );
            return Results.Ok(successResponse);
        }

    }
}
