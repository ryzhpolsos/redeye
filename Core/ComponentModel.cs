using System;
using System.Linq;
using System.Collections.Generic;

namespace RedEye.Core {
    public class ComponentManager {
        Dictionary<Type, IComponent> components = new();
        Dictionary<Type, Type> registeredComponents = new();

        public ComponentManager(){}

        public ComponentManager(IDictionary<Type, IComponent> componentList){
            foreach(var kvp in componentList){
                components.Add(kvp.Key, kvp.Value);
            }
        }

        public ComponentManager AddComponent<T>(T component) where T: IComponent {
            component.SetManager(this);
            components.Add(typeof(T), component);
            return this;
        }

        public T GetComponent<T>() where T: IComponent {
            return (T)components[typeof(T)];
        }

        public void InitializeComponents(){
            foreach(var component in components.Values){
                component.Initialize();
            }
        }

        public ComponentManager RegisterComponentType<T>(Type realization) where T: IComponent {
            registeredComponents.Add(typeof(T), realization);
            return this;
        }

        public T CreateInstance<T>(object[] args) where T: IComponent {
            var component = (T)Activator.CreateInstance(registeredComponents[typeof(T)], args);
            component.SetManager(this);
            component.Initialize();
            return component;
        }

        public T CreateInstance<T>() where T: IComponent {
            return CreateInstance<T>(new object[0]);
        }
    }

    public interface IComponent {
        public void Initialize();
        public void SetManager(ComponentManager manager);
    }
}