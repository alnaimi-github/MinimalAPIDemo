using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MinimalAPIDemo;
using MinimalAPIDemo.Data;
using MinimalAPIDemo.Models;
using MinimalAPIDemo.Models.DTO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(MappingConfig));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("/api/GetAllCoupon", () =>
{
    return Results.Ok(CouponStore.CouponsList);
}).Produces<IEnumerable<Coupon>>(200);

app.MapGet("/api/GetCouponById/{id}", (int id) =>
{
    return Results.Ok(CouponStore.CouponsList.FirstOrDefault(x => x.Id == id));
}).WithName("GetCoupon").Produces<Coupon>(200);

app.MapPost("/api/CouponCreate", (IMapper _mapper,[FromBody] CouponCreateDTO couponDto) =>
{
    if (string.IsNullOrEmpty(couponDto.name))
    {
        return Results.BadRequest("Invalid Id Or Name Coupon");
    }
    if (CouponStore.CouponsList.FirstOrDefault(x => x.Name.ToLower() == couponDto.name.ToLower()) != null)
    {
        return Results.BadRequest("Coupon Name Already Exists.");
    }
    var coupon = _mapper.Map<Coupon>(couponDto);
    coupon.Id = CouponStore.CouponsList.OrderByDescending(x => x.Id).FirstOrDefault()!.Id + 1;
    CouponStore.CouponsList.Add(coupon);
    var couponDTO =_mapper.Map<CouponDTO>(coupon);
    return Results.CreatedAtRoute("GetCoupon", new {Id=coupon.Id},couponDTO);
}).Produces<CouponDTO>(201).Produces(400).Accepts<CouponCreateDTO>("application/json");
app.MapPut("/api/CouponEdit", (Coupon coupon) =>
{

});
app.MapDelete("/api/CouponDelete/{id}", async (int id) =>
{

});

app.UseHttpsRedirection();
app.Run();
