using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Data.Notification;
using VirtoCommerce.Platform.Data.Repositories;
using VirtoCommerce.Platform.Tests.Fixtures;
using Xunit;

namespace VirtoCommerce.Platform.Tests
{
	public class NotificationScenarios : IClassFixture<RepositoryDatabaseFixture<PlatformRepository, PlatformDatabaseInitializer>>
    {
        private readonly RepositoryDatabaseFixture<PlatformRepository, PlatformDatabaseInitializer> _fixture;
		public NotificationScenarios(
            RepositoryDatabaseFixture<PlatformRepository, PlatformDatabaseInitializer> fixture)
        {
            _fixture = fixture;
        }

		[Fact]
		[Trait("Category", "Notifications")]
		public void CreateNotitfication()
		{
			var repository = _fixture.Db;
			Func<IPlatformRepository> func = () => repository;
			var service = new NotificationTemplateServiceImpl(func);
			var template = service.Create(new Core.Notification.NotificationTemplate
				{
					Body = @"&lt;p&gt; Dear {{ context.firstname }} {{ context.lastname }}, you has registered on our site&lt;/p&gt; &lt;p&gt; Your e-mail  - {{ context.email }} &lt;/p&gt;",
					Subject = @"&lt;p&gt; Thanks for registration {{ context.firstname }} {{ context.lastname }}!!! &lt;/p&gt;",
					NotificationTypeId = "RegistrationNotification",
					ObjectId = "Platform",
					TemplateEngine = "Liquid",
					DisplayName = "Registration template #1"
				});



			var manager = new NotificationManager(new LiquidNotificationTemplateResolver(), repository);
			manager.SendNotification(new RegistrationNotification
			{
				AttemptCount = 0,
				Body = template.Body,
				Channel = "Email",
				Email = "eo@virtoway.com",
				FirstName = "Evgeny",
				IsActive = true,
				LastName = "Okhrimenko",
				MaxAttemptCount = 10,
				Recipient = "eo@virtoway.com",
				Sender = "someemail@virtocommerce.com",
				Subject = template.Subject,
				ObjectId = template.ObjectId
			});

			var template1 = repository.NotificationTemplates.First();
			var notification = repository.Notifications.First();

			Assert.Equal("Registration template #1", template1.DisplayName);
			Assert.True(notification.Subject.Contains("Evgeny Okhrimenko"));
		}
	}
}
