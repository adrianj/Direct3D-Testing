using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Direct3DLib
{
    /// <summary>
    /// A general purpose class for object that have a Name property.
    /// The catch is that no objects in the world can have identical names.
    /// Attempting to name an object the same name as another will automatically
    /// append a number on the end of it.
    /// </summary>
    [Serializable]
    public class NamedObject
    {
        private static Dictionary<string, NamedObject> AllObjects = new Dictionary<string, NamedObject>();

        private string mName = "object";
        public string Name { get { return mName; } 
            set {
                string name = FindNextAvailableName(value);
                mName = name;
                AllObjects.Add(name, this);
            } }

        public static string FindNextAvailableName(string name)
        {
            int i = 0;
            string n = name;
            while (AllObjects.ContainsKey(n))
            {
                n = name + i;
                i++;
            }
            return n;
        }

        public static NamedObject GetNamedObject(string name)
        {
            if (AllObjects.ContainsKey(name)) return AllObjects[name];
            return null;
        }

        public NamedObject()
        {
            Name = "object";
        }

        public NamedObject(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return base.ToString() + " {"+Name+"}";
        }

        public event EventHandler Disposed;
        public virtual void Dispose()
        {
            //There is nothing to clean.
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
        }
        private ISite m_curISBNSite;
        public virtual ISite Site
        {
            get
            {
                return m_curISBNSite;
            }
            set
            {
                m_curISBNSite = value;
            }
        }
    }
}
