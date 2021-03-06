﻿angular.module('platformWebApp')
.controller('platformWebApp.accountListController', ['$scope', 'platformWebApp.accounts', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService',
function ($scope, accounts, bladeNavigationService, dialogService) {
    //pagination settings
    $scope.pageSettings = {};
    $scope.pageSettings.totalItems = 0;
    $scope.pageSettings.currentPage = 1;
    $scope.pageSettings.numPages = 5;
    $scope.pageSettings.itemsPerPageCount = 20;

    $scope.filter = { searchKeyword: undefined };
    var selectedNode = null;

    $scope.blade.refresh = function () {
        $scope.blade.isLoading = true;
        $scope.blade.selectedAll = false;

        accounts.search({
            keyword: $scope.filter.searchKeyword,
            skipCount: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
            takeCount: $scope.pageSettings.itemsPerPageCount
        }, function (data) {
            $scope.blade.isLoading = false;

            $scope.pageSettings.totalItems = angular.isDefined(data.totalCount) ? data.totalCount : 0;
            $scope.blade.currentEntities = data.users;

            if (selectedNode != null) {
                //select the node in the new list
                angular.forEach($scope.blade.currentEntities, function (node) {
                    if (selectedNode.id === node.id) {
                        selectedNode = node;
                    }
                });
            }
        }, function (error) {
            bladeNavigationService.setError('Error ' + error.status, $scope.blade);
        });
    };

    $scope.blade.selectNode = function (node) {
        selectedNode = node;
        $scope.selectedNodeId = selectedNode.userName;

        var newBlade = {
            id: 'listItemChild',
            data: selectedNode,
            title: selectedNode.userName,
            subtitle: $scope.blade.subtitle,
            controller: 'platformWebApp.accountDetailController',
            template: 'Scripts/app/security/blades/account-detail.tpl.html'
        };

        bladeNavigationService.showBlade(newBlade, $scope.blade);
    };

    $scope.toggleAll = function () {
        angular.forEach($scope.blade.currentEntities, function (item) {
            item.selected = $scope.blade.selectedAll;
        });
    };

    function isItemsChecked() {
        return $scope.blade.currentEntities && _.any($scope.blade.currentEntities, function (x) { return x.selected; });
    }

    function deleteChecked() {
        var dialog = {
            id: "confirmDeleteItem",
            title: "Delete confirmation",
            message: "Are you sure you want to delete selected Accounts?",
            callback: function (remove) {
                if (remove) {
                    closeChildrenBlades();

                    var selection = _.where($scope.blade.currentEntities, { selected: true });
                    var itemIds = _.pluck(selection, 'userName');
                    accounts.remove({ names: itemIds }, function (data, headers) {
                        $scope.blade.refresh();
                    }, function (error) {
                        bladeNavigationService.setError('Error ' + error.status, $scope.blade);
                    });
                }
            }
        }
        dialogService.showConfirmationDialog(dialog);
    }

    function closeChildrenBlades() {
        angular.forEach($scope.blade.childrenBlades.slice(), function (child) {
            bladeNavigationService.closeBlade(child);
        });
    }

    $scope.blade.headIcon = 'fa-lock';

    $scope.blade.toolbarCommands = [
        {
            name: "Refresh", icon: 'fa fa-refresh',
            executeMethod: function () {
                $scope.blade.refresh();
            },
            canExecuteMethod: function () {
                return true;
            }
        },
        {
            name: "Add", icon: 'fa fa-plus',
            executeMethod: function () {
                closeChildrenBlades();

                var newBlade = {
                    id: 'listItemChild',
                    currentEntity: { roles: [], userType: 'SiteAdministrator' },
                    title: 'New Account',
                    subtitle: $scope.blade.subtitle,
                    controller: 'platformWebApp.newAccountWizardController',
                    template: 'Scripts/app/security/wizards/newAccount/new-account-wizard.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, $scope.blade);
            },
            canExecuteMethod: function () {
                return true;
            },
            permission: 'platform:security:manage'
        },
        {
            name: "Delete", icon: 'fa fa-trash-o',
            executeMethod: function () {
                deleteChecked();
            },
            canExecuteMethod: function () {
                return isItemsChecked();
            },
            permission: 'platform:security:manage'
        }
    ];

    $scope.$watch('pageSettings.currentPage', function () {
        $scope.blade.refresh();
    });

    // actions on load
    //No need to call this because page 'pageSettings.currentPage' is watched!!! It would trigger subsequent duplicated req...
    //$scope.blade.refresh();
}]);