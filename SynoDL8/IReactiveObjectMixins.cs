// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved


using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using ReactiveUI;

namespace SynoDL8
{
    /// <summary>
    /// Helps with restorable state for ReactiveObject.
    /// 
    /// code taken from ViewModel.cs in Prism.StoreApps
    /// http://prismwindowsruntime.codeplex.com/SourceControl/latest#8.1/Prism.StoreApps/ViewModel.cs
    /// </summary>
    public static class ReactiveObjectMixins
    {
        /// <summary>
        /// Retrieves the entity state value of the specified entity state key.
        /// </summary>
        /// <typeparam name="T">Type of the expected return value</typeparam>
        /// <param name="entityStateKey">The entity state key.</param>
        /// <param name="viewModelState">State of the view model.</param>
        /// <returns>The T type object that represents the state value of the specified entity.</returns>
        public static T RetrieveEntityStateValue<T>(string entityStateKey, IDictionary<string, object> viewModelState)
        {
            if (viewModelState != null && viewModelState.ContainsKey(entityStateKey))
            {
                return (T)viewModelState[entityStateKey];
            }

            return default(T);
        }

        /// <summary>
        /// Adds an entity state value to the view model state dictionary.
        /// </summary>
        /// <param name="viewModelStateKey">The view model state key.</param>
        /// <param name="viewModelStateValue">The view model state value.</param>
        /// <param name="viewModelState">The view model state dictionary.</param>
        public static void AddEntityStateValue(string viewModelStateKey, object viewModelStateValue, IDictionary<string, object> viewModelState)
        {
            if (viewModelState != null)
            {
                viewModelState[viewModelStateKey] = viewModelStateValue;
            }
        }

        public static void FillStateDictionary(this ReactiveObject viewModel, IDictionary<string, object> viewModelState)
        {
            var viewModelProperties = viewModel.GetType().GetRuntimeProperties().Where(
                                                            c => c.GetCustomAttribute(typeof(RestorableStateAttribute)) != null);

            foreach (PropertyInfo propertyInfo in viewModelProperties)
            {
                viewModelState[propertyInfo.Name] = propertyInfo.GetValue(viewModel);
            }
        }

        public static void RestoreViewModel(this ReactiveObject viewModel, IDictionary<string, object> viewModelState)
        {
            var viewModelProperties = viewModel.GetType().GetRuntimeProperties().Where(
                                                            c => c.GetCustomAttribute(typeof(RestorableStateAttribute)) != null);

            foreach (PropertyInfo propertyInfo in viewModelProperties)
            {
                if (viewModelState.ContainsKey(propertyInfo.Name))
                {
                    propertyInfo.SetValue(viewModel, viewModelState[propertyInfo.Name]);
                }
            }
        }
    }
}
