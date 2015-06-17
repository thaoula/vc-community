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
			var service = new NotificationTemplateServiceImpl(repository);
			var template = service.Create(new Core.Notification.NotificationTemplate
				{
					Body = @"&lt;p&gt; Dear {{ context.first_name }} {{ context.last_name }}, you has registered on our site&lt;/p&gt; &lt;p&gt; Your e-mail  - {{ context.email }} &lt;/p&gt;",
					Subject = @"&lt;p&gt; Thanks for registration {{ context.first_name }} {{ context.last_name }}!!! &lt;/p&gt;",
					NotificationTypeId = "RegistrationNotification",
					ObjectId = "Platform",
					TemplateEngine = "Liquid",
					DisplayName = "Registration template #1"
				});


			repository = _fixture.Db;
			var manager = new NotificationManager(new LiquidNotificationTemplateResolver(), repository);

			var notification = new RegistrationNotification
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
			};

			manager.SendNotification(notification);

			var templatesC = repository.NotificationTemplates.Count();
			var notificationC = repository.Notifications.Count();

			Assert.Equal("Registration template #1", template.DisplayName);
			Assert.True(notification.Subject.Contains("Evgeny Okhrimenko"));
		}
	}
}
