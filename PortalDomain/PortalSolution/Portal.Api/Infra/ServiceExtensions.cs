﻿using DotNetCore.CAP;
using FluentValidation;
using FluentValidation.AspNetCore;

using Marten;

using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Portal.Api.External.Consumers;
using Portal.Api.External.Producers;
using Wolverine;
using Wolverine.Marten;

namespace Portal.Api.Infra;

public static class ServicesExtensions
{
    public static IServiceCollection AddPortalInfraServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var environment = builder.Environment;
        var config = builder.Configuration;

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSignalR();
        services.AddHttpContextAccessor();

        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationRulesToSwagger();

        services.AddScoped<IConsumeMessagesFromTheSoftwareDomain, KafkaSoftwareDomainConsumer>();

        services.AddAuthentication().AddJwtBearer(options =>
        {
            if (environment.IsDevelopment())
            {
                options.RequireHttpsMetadata = false;
            }
        });

        services.AddCors(options =>
        {
            options.AddPolicy("cors", (pol =>
            {
                var origins = config.GetSection("allowed-origins").Get<string[]>() ?? Array.Empty<string>();
                pol.WithOrigins(origins);
                pol.AllowCredentials();
                pol.AllowAnyMethod();
                pol.AllowAnyHeader();
            }));
        });

        var connectionString = config.GetConnectionString("portal") ?? throw new Exception("Need Connection String");


        var marten = services.AddMarten(options =>
        {
            options.Connection(connectionString);
            if (environment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = Weasel.Core.AutoCreate.All;
            }
        }).IntegrateWithWolverine()
          .UseLightweightSessions();

        if (environment.IsProduction())
        {
            marten.OptimizeArtifactWorkflow();
        }
        var kafkaConnectionString = config.GetConnectionString("kafka") ?? throw new Exception("Need a Kafka Broker");

        void CapSetupActionsProducer(CapOptions options)
        {
            options.UseKafka(kafkaConnectionString);
            options.UsePostgreSql(connectionString);
        }
        void CapSetupActionsConsumer(CapOptions options)
        {
            options.UseKafka(kafkaConnectionString);
            options.UsePostgreSql(connectionString);
            options.UseDashboard();
        }
        services.AddCap(CapSetupActionsConsumer);
        builder.Host.UseWolverine(opts =>
        {
            opts.Services.AddMarten(connectionString)
            .IntegrateWithWolverine().UseLightweightSessions();
            opts.Policies.AutoApplyTransactions();
            Console.WriteLine(opts.DescribeHandlerMatch(typeof(KafkfaUserHandler)));
            opts.Services.AddCap(CapSetupActionsProducer);

        });


   
        return services;
    }
}