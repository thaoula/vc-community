﻿angular.module('platformWebApp')
.controller('platformWebApp.settingDictionaryController', ['$scope', 'platformWebApp.dialogService', 'platformWebApp.bladeNavigationService', 'platformWebApp.settings', function ($scope, dialogService, bladeNavigationService, settings) {
    var blade = $scope.blade;
    var currentEntities;

    blade.refresh = function (parentRefresh) {
        if (blade.isApiSave) {
            settings.get({ id: blade.currentEntityId }, function (setting) {
                if (parentRefresh && blade.parentRefresh) {
                    blade.parentRefresh(setting.arrayValues);
                }

                setting.arrayValues = _.map(setting.arrayValues, function (x) { return { value: x }; });
                blade.origEntity = angular.copy(setting.arrayValues);
                initializeBlade(setting);
            },
            function (error) { bladeNavigationService.setError('Error ' + error.status, blade); });
        }
    }

    function initializeBlade(data) {
        blade.title = data.title;
        blade.currentEntity = data;
        currentEntities = blade.currentEntity.arrayValues;
        blade.isLoading = false;
    }

    $scope.dictValueValidator = function (value) {
        if (blade.currentEntity) {
            if (blade.currentEntity.valueType == 'ShortText') {
                return _.all(currentEntities, function (item) { return angular.lowercase(item.value) !== angular.lowercase(value); });
            } else {
                return _.all(currentEntities, function (item) { return item.value !== value; });
            }
        }
        return false;
    };

    $scope.add = function (form) {
        if (form.$valid) {
            currentEntities.push($scope.newValue);
            resetNewValue();
            form.$setPristine();
        }
    };

    $scope.delete = function (index) {
        currentEntities.splice(index, 1);
        $scope.selectedItem = undefined;
    };

    $scope.selectItem = function (listItem) {
        $scope.selectedItem = listItem;
    };

    blade.headIcon = 'fa-wrench';
    blade.subtitle = 'Manage dictionary values';
    blade.toolbarCommands = [
     {
         name: "Delete", icon: 'fa fa-trash-o',
         executeMethod: function () {
             deleteChecked();
         },
         canExecuteMethod: function () {
             return isItemsChecked();
         }
     }
    ];

    if (blade.isApiSave) {
        var formScope;
        $scope.setForm = function (form) {
            formScope = form;
        }

        function isDirty() {
            return !angular.equals(currentEntities, blade.origEntity);
        };

        function saveChanges() {
            blade.selectedAll = false;
            blade.isLoading = true;
            blade.currentEntity.arrayValues = _.pluck(blade.currentEntity.arrayValues, 'value');

            settings.update(null, [blade.currentEntity], blade.refresh,
                function (error) { bladeNavigationService.setError('Error ' + error.status, $scope.blade); });
        };

        blade.toolbarCommands.splice(0, 0,
        {
            name: "Save",
            icon: 'fa fa-save',
            executeMethod: function () {
                saveChanges();
            },
            canExecuteMethod: function () {
                return isDirty() && formScope && formScope.$valid;
            }
        },
        {
            name: "Reset",
            icon: 'fa fa-undo',
            executeMethod: function () {
                angular.copy(blade.origEntity, currentEntities);
                blade.selectedAll = false;
            },
            canExecuteMethod: function () {
                return isDirty();
            }
        });
    }

    $scope.checkAll = function () {
        angular.forEach(currentEntities, function (item) {
            item._selected = blade.selectedAll;
        });
    };

    function resetNewValue() {
        $scope.newValue = { value: null };
    }

    function isItemsChecked() {
        return _.any(currentEntities, function (x) { return x._selected; });
    }

    function deleteChecked() {
        var dialog = {
            id: "confirmDeleteItem",
            title: "Delete confirmation",
            message: "Are you sure you want to delete selected Values?",
            callback: function (remove) {
                if (remove) {
                    var selection = _.where(currentEntities, { _selected: true });
                    angular.forEach(selection, function (listItem) {
                        $scope.delete(currentEntities.indexOf(listItem));
                    });
                }
            }
        }
        dialogService.showConfirmationDialog(dialog);
    }

    $scope.$watch('blade.parentBlade.currentEntities', function (data) {
        if (data) {
            var allEntities = _.flatten(_.map(data, _.values));
            initializeBlade(_.findWhere(allEntities, { name: blade.currentEntityId }));
        }
    });

    // on load
    resetNewValue();
    blade.refresh();
}]);