using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalAPIDemo;
using MinimalAPIDemo.Data;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.DTO;
using MinimalAPIDemo.Shared;
using System.Net;
using MinimalAPIDemo.Repository.IRepository;
using MinimalAPIDemo.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddDbContext<ApplicationDbContext>(op =>
{
    op.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionStringDb"));
});

builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("/api/GetAllCoupon", async (ICouponRepository couponRepository) =>
{
    var response = new APIResponse(
        IsSuccess: true,
        Result: await couponRepository.GetAllAsync(),
        StatusCode: HttpStatusCode.OK
    );
    return Results.Ok(response);
}).Produces<APIResponse>(200);


app.MapGet("/api/GetCouponById/{id}",async (ICouponRepository couponRepository, int id) =>
{
    var response = new APIResponse(
       IsSuccess: true,
       Result: await couponRepository.GetAsync(id),
       StatusCode: HttpStatusCode.OK
   );
    return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);

app.MapPost("/api/CouponCreate", async (ICouponRepository couponRepository, IMapper _mapper, IValidator<CouponCreateDTO> _validator, [FromBody] CouponCreateDTO couponDto) =>
{
    var response = new APIResponse(
        IsSuccess: false,
        StatusCode: HttpStatusCode.BadRequest,
        ErrorMessages: []
    );

    var validationResult = await _validator.ValidateAsync(couponDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault()?.ToString() ?? "Validation error");
        return Results.BadRequest(response);
    }

    if (await couponRepository.GetAsync(couponDto.Name) is not null)
    {
        response.ErrorMessages.Add("Coupon Name Already Exists.");
        return Results.BadRequest(response);
    }

    var coupon = _mapper.Map<Coupon>(couponDto);
    await couponRepository.CreateAsync(coupon);
    await couponRepository.SaveAsync();

    var couponDTO = _mapper.Map<CouponDTO>(coupon);
    response = new APIResponse(
        IsSuccess: true,
        Result: couponDTO,
        StatusCode: HttpStatusCode.Created
    );

    return Results.CreatedAtRoute("GetCoupon", new { Id = coupon.Id }, response);
})
.Produces<APIResponse>(201)
.Produces<APIResponse>(400)
.Accepts<CouponCreateDTO>("application/json");

app.MapPut("/api/CouponUpdate", async (ICouponRepository couponRepository, IMapper _mapper, IValidator<CouponUpdateDTO> _validator, [FromBody] CouponUpdateDTO couponDto) =>
{
    var response = new APIResponse(
        IsSuccess: false,
        Result: null,
        StatusCode: HttpStatusCode.BadRequest,
        ErrorMessages: []
    );

    var validationResult = await _validator.ValidateAsync(couponDto);
    if (!validationResult.IsValid)
    {
        response.ErrorMessages!.Add(validationResult.Errors.FirstOrDefault()?.ToString() ?? "Validation error");
        return Results.BadRequest(response);
    }

    var existingCoupon =await couponRepository.GetAsync(couponDto.Id);
    if (existingCoupon is null)
    {
        response.ErrorMessages!.Add("Coupon not found.");
        return Results.NotFound(response);
    }

    var obj= _mapper.Map(couponDto, existingCoupon);
    await couponRepository.UpdateAsync(obj);
    await couponRepository.SaveAsync();

    response = new APIResponse(
        IsSuccess: true,
        Result: _mapper.Map<CouponDTO>(existingCoupon),
        StatusCode: HttpStatusCode.OK
    );

    return Results.Ok(response);
})
.Produces<APIResponse>(200)
.Produces<APIResponse>(400)
.Produces<APIResponse>(404)
.Accepts<CouponUpdateDTO>("application/json");




app.MapDelete("/api/CouponDelete/{id:int}", async (ICouponRepository couponRepository, int id) =>
{
    var response = new APIResponse(
        IsSuccess: false,
        StatusCode: HttpStatusCode.NotFound,
        ErrorMessages: []
    );

    var couponResult =await couponRepository.GetAsync(id);
    if (couponResult is null)
    {
        response.ErrorMessages.Add("Coupon not found.");
        return Results.NotFound(response);
    }
    else
    {
       await couponRepository.DeleteAsync(couponResult);
       await couponRepository.SaveAsync();
    }

    response = new APIResponse(
        IsSuccess: true,
        StatusCode: HttpStatusCode.NoContent
    );

    return Results.NoContent();
})
.Produces<APIResponse>(204)
.Produces<APIResponse>(404);


app.UseHttpsRedirection();
app.Run();
