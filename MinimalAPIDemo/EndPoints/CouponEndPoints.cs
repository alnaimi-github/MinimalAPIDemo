using System.Net;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.DTO;
using MinimalAPIDemo.Repository.IRepository;
using MinimalAPIDemo.Shared;

namespace MinimalAPIDemo.EndPoints;

public static class CouponEndPoints
{
    public static void ConfigureCouponEndPoints(this WebApplication app)
    {
        app.MapGet("/api/getCoupons", GetAllCouponAsync)
            .WithName("GetCoupons")
            .Produces<APIResponse>()
            .Produces<APIResponse>(statusCode:(int)HttpStatusCode.Unauthorized)
            .RequireAuthorization("AdminOnly");


        app.MapGet("/api/getCouponById/{id:int}", GetCouponByIdAsync)
            .WithName("GetCouponById")
            .Produces<APIResponse>()
            .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Unauthorized)
            .RequireAuthorization();

        app.MapPost("/api/createCoupon", CreateCouponAsync)
            .Produces<APIResponse>(201)
            .Produces<APIResponse>(404)
            .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Unauthorized)
            .Accepts<CouponCreateDTO>("application/json")
            .WithName("CreateCoupon")
            .RequireAuthorization();


        app.MapPut("/api/updateCoupon", UpdateCouponAsync)
            .Produces<APIResponse>()
            .Produces<APIResponse>(400)
            .Produces<APIResponse>(404)
            .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Unauthorized)
            .Accepts<CouponUpdateDTO>("application/json")
            .WithName("UpdateCoupon")
            .RequireAuthorization();


        app.MapDelete("/api/deleteCoupon/{id:int}", DeleteCouponAsync)
            .Produces<APIResponse>(204)
            .Produces<APIResponse>(404)
            .Produces<APIResponse>(statusCode: (int)HttpStatusCode.Unauthorized)
            .WithName("DeleteCoupon")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetAllCouponAsync(ICouponRepository couponRepository, ILogger<Program> logger)
    {
        logger.Log(LogLevel.Information, "Getting All Coupons");
        var response = new APIResponse(
            true,
            await couponRepository.GetAllAsync(),
            HttpStatusCode.OK
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> GetCouponByIdAsync(ICouponRepository couponRepository, int id,
        ILogger<Program> logger)
    {
        logger.Log(LogLevel.Information, "Getting Coupon By Id");
        var response = new APIResponse(
            true,
            await Task.FromResult(couponRepository.GetAsync(id)),
            HttpStatusCode.OK
        );
        return Results.Ok(response);
    }

    private static async Task<IResult> CreateCouponAsync(ICouponRepository couponRepository, IMapper mapper,
        IValidator<CouponCreateDTO> validator, [FromBody] CouponCreateDTO couponDto, ILogger<Program> logger)
    {
        logger.Log(LogLevel.Information, "Creating New Coupon");
        var response = new APIResponse(
            false,
            StatusCode: HttpStatusCode.BadRequest,
            ErrorMessages: []
        );

        var validationResult = await validator.ValidateAsync(couponDto);
        if (!validationResult.IsValid)
        {
            response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()?.ToString() ??
                                       "Validation error");
            return Results.BadRequest(response);
        }

        if (await couponRepository.GetAsync(couponDto.Name) is not null)
        {
            response.ErrorMessages.Add("Coupon Name Already Exists.");
            return Results.BadRequest(response);
        }

        var coupon = mapper.Map<Coupon>(couponDto);
        await couponRepository.CreateAsync(coupon);
        await couponRepository.SaveAsync();

        var couponDtoRMap = mapper.Map<CouponDTO>(coupon);
        response = new APIResponse(
            true,
            couponDtoRMap,
            HttpStatusCode.Created
        );

        return Results.CreatedAtRoute("GetCoupon", new { coupon.Id }, response);
    }

    private static async Task<IResult> UpdateCouponAsync(ICouponRepository couponRepository, IMapper mapper,
        IValidator<CouponUpdateDTO> validator, [FromBody] CouponUpdateDTO couponDto, ILogger<Program> logger)
    {
        logger.Log(LogLevel.Information, "Updating Coupon");
        var response = new APIResponse(
            false,
            null!,
            HttpStatusCode.BadRequest,
            []
        );

        var validationResult = await validator.ValidateAsync(couponDto);
        if (!validationResult.IsValid)
        {
            response.ErrorMessages!.Add(validationResult.Errors.FirstOrDefault()?.ToString() ??
                                        "Validation error");
            return Results.BadRequest(response);
        }

        var existingCoupon = await couponRepository.GetAsync(couponDto.Id);
        if (existingCoupon is null)
        {
            response.ErrorMessages!.Add("Coupon not found.");
            return Results.NotFound(response);
        }

        var obj = mapper.Map(couponDto, existingCoupon);
        await couponRepository.UpdateAsync(obj);
        await couponRepository.SaveAsync();

        response = new APIResponse(
            true,
            mapper.Map<CouponDTO>(existingCoupon),
            HttpStatusCode.OK
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteCouponAsync(ICouponRepository couponRepository, int id,
        ILogger<Program> logger)
    {
        logger.Log(LogLevel.Information, "Deleted Coupon Successfully!");
        var response = new APIResponse(
            false,
            StatusCode: HttpStatusCode.NotFound,
            ErrorMessages: []
        );

        var couponResult = await couponRepository.GetAsync(id);
        if (couponResult is null)
        {
            response.ErrorMessages.Add("Coupon not found.");
            return Results.NotFound(response);
        }

        await couponRepository.DeleteAsync(couponResult);
        await couponRepository.SaveAsync();

        response = new APIResponse(
            true,
            StatusCode: HttpStatusCode.NoContent
        );

        return Results.NoContent();
    }
}