using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GFramework.UI
{
    public class PropertyBinder<T> where T : BaseViewModel
    {
        private delegate void BindHandler(T viewModel);
        private delegate void UnBindHandler(T viewModel);

        private readonly List<BindHandler> _binders = new List<BindHandler>();
        private readonly List<UnBindHandler> _unbinders = new List<UnBindHandler>();

        public void Add<TProperty>(string name, BindableProperty<TProperty>.OnValueChangeHandler valueChangeHandler)
        {
            // 获得该属性绑定器对应的ViewModel的字段描述
            var fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public);
            // 如果找不到对应的字段描述,抛出异常
            if (fieldInfo == null)
            {
                throw new Exception(string.Format("Unable to find bindable property field '{0}.{1}'", typeof(T).Name, name));
            }

            // 添加绑定方法
            _binders.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(name, viewModel, fieldInfo).OnValueChanged += valueChangeHandler;
            });

            _unbinders.Add((viewModel) =>
            {
                GetPropertyValue<TProperty>(name, viewModel, fieldInfo).OnValueChanged -= valueChangeHandler;
            });
        }

        private BindableProperty<TProperty> GetPropertyValue<TProperty>(string name, T viewModel, FieldInfo fieldInfo)
        {
            // value即是要获得的可绑定属性
            var value = fieldInfo.GetValue(viewModel);

            BindableProperty<TProperty> bindableProperty = value as BindableProperty<TProperty>;
            // 如果没有目标属性,抛出异常
            if (bindableProperty == null)
            {
                throw new Exception(string.Format("Illegal bindableproperty field '{0}.{1}' ", typeof(T).Name, name));
            }
            return bindableProperty;
        }

        /// <summary>
        /// 将监听方法绑定在目标属性上
        /// </summary>
        /// <param name="viewModel"></param>
        public void Bind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < _binders.Count; i++)
                {
                    _binders[i](viewModel);
                }
            }
        }

        /// <summary>
        /// 将该方法从指定的ViewModel中去除
        /// </summary>
        /// <param name="viewModel"></param>
        public void Unbind(T viewModel)
        {
            if (viewModel != null)
            {
                for (int i = 0; i < _unbinders.Count; i++)
                {
                    _unbinders[i](viewModel);
                }
            }
        }
    }
}
