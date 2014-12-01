﻿angular.module('catalogModule.wizards.importJobWizard.mapping', [
])
.controller('importJobMappingController', ['$scope', 'bladeNavigationService', function ($scope, bladeNavigationService)
{
    $scope.blade.refresh = function ()
    {
        $scope.blade.isLoading = true;

        $scope.blade.item.$updateMappingItems(null, function (result)
        {
            $scope.blade.isLoading = false;
            $scope.item = result;
        });
    };


    $scope.saveChanges = function ()
    {
        $scope.blade.parentBlade.item.propertiesMap = $scope.item.propertiesMap;
        $scope.bladeClose();
    };

    $scope.setForm = function (form)
    {
        $scope.formScope = form;
    }

    $scope.setSelectedItem = function (data)
    {
        $scope.selectedItem = data;
    }

    $scope.bladeToolbarCommands = [
        {
            name: "Edit",
            icon: 'icon-new-tab-2',
            executeMethod: function() {
                $scope.editMapping($scope.selectedItem);
            },
            canExecuteMethod: function() {
                return $scope.selectedItem;
            }
        }
    ];

    $scope.editMapping = function(column) {
        var newBlade = {
            id: 'importJobWizardMappingEdit',
            item: column,
            csvColumns: $scope.item.availableCsvColumns,
            title: column.entityColumnName,
            subtitle: 'Edit column mapping',
            controller: 'importJobMappingEditController',
            bladeActions: 'Modules/Catalog/VirtoCommerce.CatalogModule.Web/Scripts/app/catalog/wizards/common/wizard-ok-action.tpl.html',
            template: 'Modules/Catalog/VirtoCommerce.CatalogModule.Web/Scripts/app/catalog/wizards/importWizard/import-job-wizard-mapping-step-edit.tpl.html'
        };
        bladeNavigationService.showBlade(newBlade, $scope.blade);
    }

    $scope.blade.refresh();

}]);


