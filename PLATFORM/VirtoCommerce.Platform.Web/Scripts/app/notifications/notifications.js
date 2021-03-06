﻿var moduleName = "platformWebApp";

if (AppDependencies != undefined) {
	AppDependencies.push(moduleName);
}

angular.module(moduleName)
.config(
  ['$stateProvider', function ($stateProvider) {
  	$stateProvider
		.state('workspace.notifications', {
			url: '/notifications?objectId&objectTypeId',
			templateUrl: 'Scripts/common/templates/home.tpl.html',
			controller: ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
				var blade = {
					id: 'notifications',
					title: 'Notifications',
					subtitle: 'Working with notifications system',
					controller: 'platformWebApp.notificationsMenuController',
					template: 'Scripts/app/notifications/blades/notifications-menu.tpl.html',
					isClosingDisabled: true
				};
				bladeNavigationService.showBlade(blade);
			}
			]
		});
  }]
)
.run(
  ['$rootScope', 'platformWebApp.mainMenuService', 'platformWebApp.widgetService', '$state', function ($rootScope, mainMenuService, widgetService, $state) {
  	//Register module in main menu
  	var menuItem = {
  		path: 'configuration/notifications',
  		icon: 'fa fa-envelope',
  		title: 'Notifications',
  		priority: 7,
  		action: function () { $state.go('workspace.notifications'); },
  		//permission: 'platform:module:query'
  	};
  	mainMenuService.addMenuItem(menuItem);
  }])
;