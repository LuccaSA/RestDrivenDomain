using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rdd.Application;
using Rdd.Domain.Json;
using Rdd.Domain.Models.Querying;
using Rdd.Domain.Rights;
using Rdd.Infra.Storage;
using Rdd.Web.Helpers;
using Rdd.Web.Querying;
using Rdd.Web.Tests.Models;
using Rdd.Web.Tests.ServerMock;
using Xunit;

namespace Rdd.Web.Tests
{
    public class BeforeAfterSaveChangesValidation
    {
        private class OptionsAccessor : IOptions<MvcJsonOptions>
        {
            public static MvcJsonOptions JsonOptions = new MvcJsonOptions();
            public MvcJsonOptions Value => JsonOptions;
        }

        [Fact]
        public async Task MultipleImplementations()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ExchangeRateDbContext>((service, options) => { options.UseInMemoryDatabase("BeforeAfterSave"); });
            services.AddRdd<ExchangeRateDbContext>(rdd =>
                {
                    rdd.PagingLimit = 10;
                    rdd.PagingMaximumLimit = 4242;
                })
                .WithDefaultRights(RightDefaultMode.Open)
                .AddOnSaveChangesHook<OnExchangeRateSave, ExchangeRate>()
                .AddOnSaveChangesHook<OnAnotherUserSave, AnotherUser>();

            var provider = services.BuildServiceProvider();

            var app = provider.GetRequiredService<IAppController<ExchangeRate, int>>();
            var create = new CandidateParser(new JsonParser(), new OptionsAccessor()).Parse<ExchangeRate, int>(@"{ ""name"": ""new name"" }");

            ExchangeRate created = await app.CreateAsync(create, new Query<ExchangeRate>());

            var onSave = provider.GetRequiredService<IOnSaveChangesHookAsync<ExchangeRate>>() as IOnSaveCount;
            var onAnotherSave = provider.GetRequiredService<IOnSaveChangesHookAsync<AnotherUser>>() as IOnSaveCount;

            Assert.Equal(2, onSave.AddCount);
            Assert.Equal(0, onSave.UpdateCount);
            Assert.Equal(0, onSave.DeleteCount);
            Assert.Equal(2, onSave.CallsCount);

            Assert.Equal(0, onAnotherSave.AddCount);
            Assert.Equal(0, onAnotherSave.UpdateCount);
            Assert.Equal(0, onAnotherSave.DeleteCount);
            Assert.Equal(0, onAnotherSave.CallsCount);

            var update = new CandidateParser(new JsonParser(), new OptionsAccessor()).Parse<ExchangeRate, int>(@"{ ""name"": ""other name"" }");
            var updated = await app.UpdateByIdAsync(created.Id, update, new Query<ExchangeRate>());

            Assert.Equal(2, onSave.AddCount);
            Assert.Equal(2, onSave.UpdateCount);
            Assert.Equal(0, onSave.DeleteCount);
            Assert.Equal(4, onSave.CallsCount);

            await app.DeleteByIdAsync(updated.Id);

            Assert.Equal(2, onSave.AddCount);
            Assert.Equal(2, onSave.UpdateCount);
            Assert.Equal(2, onSave.DeleteCount);
            Assert.Equal(6, onSave.CallsCount);

            Assert.Equal(0, onAnotherSave.CallsCount);
        }
    }

    public class OnExchangeRateSave : SaveEventProcessorMock<ExchangeRate>
    {
    }

    public class OnAnotherUserSave : SaveEventProcessorMock<AnotherUser>
    {
    }

    public interface IOnSaveCount {
        int AddCount { get; }
        int UpdateCount { get; }
        int DeleteCount { get; }
        int CallsCount { get; }
    }

    public class SaveEventProcessorMock<T> : IOnSaveChangesHookAsync<T>, IOnSaveCount where T : class
    {
        public int AddCount { get; private set; } = 0;
        public int UpdateCount { get; private set; } = 0;
        public int DeleteCount { get; private set; } = 0;
        public int CallsCount { get; private set; } = 0;

        public Task OnBeforeSaveAsync(SavedEntries<T> savedEntries)
        {
            UpdateStats(savedEntries);
            return Task.CompletedTask;
        }

        public Task OnAfterSaveAsync(SavedEntries<T> savedEntries)
        {
            UpdateStats(savedEntries);
            return Task.CompletedTask;
        }

        private void UpdateStats(SavedEntries<T> savedEntries)
        {
            CallsCount++;
            if (savedEntries.Added.Any())
            {
                AddCount++;
            }
            if (savedEntries.Modified.Any())
            {
                UpdateCount++;
            }
            if (savedEntries.Deleted.Any())
            {
                DeleteCount++;
            }
        }
    }
}