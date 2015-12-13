﻿using System;

namespace Stashbox.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
    public class DependencyAttribute : Attribute
    {
        public string Name { get; set; }

        public DependencyAttribute(string name = null)
        {
            this.Name = name;
        }
    }
}